using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GameDevKit.Editor.AppBuild
{
    [Serializable]
    public class AndroidBuildConfig : BuildConfig
    {
        public AndroidBuildSettings AndroidBuildSettings;
        [Space] public bool AutoIncrementBundleVersionCode;

        private AndroidBuildSettings _originalSettings;

        public AndroidBuildConfig()
        {
            BuildOptions = BuildOptions.Development | BuildOptions.CompressWithLz4HC;
            BuildTarget = BuildTarget.Android;
            BuildTargetGroup = BuildTargetGroup.Android;
            BuildSuffix = "-dev";
        }

        public override async UniTask PreBuildAsync()
        {
            _originalSettings = AndroidBuildSettings.GetCurrentSettings();
            AndroidBuildSettings.Apply();
            Debug.Log($"Applied Android build settings:\n{AndroidBuildSettings.ToJsonUnity()}");
            await UniTask.SwitchToMainThread();
        }

        public override async UniTask PostBuildAsync(BuildReport buildReport)
        {
            _originalSettings.Apply();
            Debug.Log($"Restored original Android build settings:\n{_originalSettings.ToJsonUnity()}");

            if (AutoIncrementBundleVersionCode)
            {
                var prev = AndroidBuildSettings.BundleVersionCode;
                AndroidBuildSettings.BundleVersionCode++;
                Debug.Log($"BundleVersionCode increased from {prev} -> {AndroidBuildSettings.BundleVersionCode}");
            }
            await UniTask.SwitchToMainThread();
        }

        public override string GetBuildPath()
        {
            var buildName = BuildNameOverride.IsNullOrEmpty() ? Application.productName : BuildNameOverride;
            var version = $"_{AndroidBuildSettings.BundleVersion}_{AndroidBuildSettings.BundleVersionCode}";
            return $"{BuildFolder}/{BuildTarget}/{buildName}{BuildSuffix}{version}{GetExtension()}";
        }

        public override string GetExtension() => AndroidBuildSettings.BuildAppBundle ? ".aab" : ".apk";
    }

    [Serializable]
    public class AndroidBuildSettings
    {
        public bool BuildAppBundle;
        public string BundleVersion;
        public int BundleVersionCode;
        public AndroidSdkVersions MinSdkVersion;
        public AndroidSdkVersions TargetSdkVersion;
        public bool UseCustomKeystore;
        public string KeystorePath;
        public string KeystorePass;
        public string KeyAliasName;
        public string KeyAliasPass;

        public void Apply()
        {
            EditorUserBuildSettings.buildAppBundle = BuildAppBundle;
            PlayerSettings.Android.bundleVersionCode = BundleVersionCode;
            PlayerSettings.Android.minSdkVersion = MinSdkVersion;
            PlayerSettings.Android.targetSdkVersion = TargetSdkVersion;
            PlayerSettings.Android.useCustomKeystore = UseCustomKeystore;
            PlayerSettings.Android.keystoreName = KeystorePath;
            PlayerSettings.Android.keystorePass = KeystorePass;
            PlayerSettings.Android.keyaliasName = KeyAliasName;
            PlayerSettings.Android.keyaliasPass = KeyAliasPass;
        }

        public static AndroidBuildSettings GetCurrentSettings()
        {
            return new AndroidBuildSettings
            {
                BuildAppBundle = EditorUserBuildSettings.buildAppBundle,
                BundleVersionCode = PlayerSettings.Android.bundleVersionCode,
                MinSdkVersion = PlayerSettings.Android.minSdkVersion,
                TargetSdkVersion = PlayerSettings.Android.targetSdkVersion,
                UseCustomKeystore = PlayerSettings.Android.useCustomKeystore,
                KeystorePath = PlayerSettings.Android.keystoreName,
                KeystorePass = PlayerSettings.Android.keystorePass,
                KeyAliasName = PlayerSettings.Android.keyaliasName,
                KeyAliasPass = PlayerSettings.Android.keyaliasPass,
            };
        }
    }
}
