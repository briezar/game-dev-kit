using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.AppBuild
{
    [Serializable]
    public class AndroidBuildConfig : BuildConfig
    {
        public AndroidBuildSettings AndroidBuildSettings;

        private AndroidBuildSettings _originalSettings;

        public override async UniTask PreBuildAsync()
        {
            _originalSettings = AndroidBuildSettings.GetCurrentSettings();
            AndroidBuildSettings.Apply();
            Debug.Log($"Applied Android build settings:\n{AndroidBuildSettings.ToJsonUnity()}");
            await UniTask.SwitchToMainThread();
        }

        public override async UniTask PostBuildAsync()
        {
            _originalSettings.Apply();
            Debug.Log($"Restored original Android build settings:\n{_originalSettings.ToJsonUnity()}");
            await UniTask.SwitchToMainThread();
        }

        public override BuildPlayerOptions GetBuildPlayerOptions()
        {
            AndroidBuildSettings.Apply();
            return base.GetBuildPlayerOptions();
        }

        public override string GetExtension() => AndroidBuildSettings.BuildAppBundle ? ".aab" : ".apk";
    }

    [Serializable]
    public class AndroidBuildSettings
    {
        public bool BuildAppBundle;
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
