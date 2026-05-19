using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.AppBuild
{
    [Serializable]
    public class iOSBuildConfig : BuildConfig
    {
        public iOSBuildSettings iOSBuildSettings;

        private iOSBuildSettings _originalSettings;

        public override async UniTask PreBuildAsync()
        {
            _originalSettings = iOSBuildSettings.GetCurrentSettings();
            iOSBuildSettings.Apply();
            Debug.Log($"Applied iOS build settings:\n{iOSBuildSettings.ToJsonUnity()}");
            await UniTask.SwitchToMainThread();
        }

        public override async UniTask PostBuildAsync()
        {
            _originalSettings.Apply();
            Debug.Log($"Restored original iOS build settings:\n{_originalSettings.ToJsonUnity()}");
            await UniTask.SwitchToMainThread();
        }

        public override BuildPlayerOptions GetBuildPlayerOptions()
        {
            iOSBuildSettings.Apply();
            return base.GetBuildPlayerOptions();
        }
    }

    [Serializable]
    public class iOSBuildSettings
    {
        public string BuildNumber;
        public string TargetOSVersion;
        public iOSTargetDevice TargetDevice;
        public iOSSdkVersion SdkVersion;
        public bool AppleEnableAutomaticSigning;
        public string AppleDeveloperTeamID;
        public string ManualProvisioningProfileID;
        public ProvisioningProfileType ManualProvisioningProfileType;
        public XcodeBuildConfig XcodeBuildConfig;

        public void Apply()
        {
            PlayerSettings.iOS.buildNumber = BuildNumber;
            PlayerSettings.iOS.targetOSVersionString = TargetOSVersion;
            PlayerSettings.iOS.targetDevice = TargetDevice;
            PlayerSettings.iOS.sdkVersion = SdkVersion;
            PlayerSettings.iOS.appleEnableAutomaticSigning = AppleEnableAutomaticSigning;
            PlayerSettings.iOS.appleDeveloperTeamID = AppleDeveloperTeamID;
            PlayerSettings.iOS.iOSManualProvisioningProfileID = ManualProvisioningProfileID;
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ManualProvisioningProfileType;
            EditorUserBuildSettings.iOSXcodeBuildConfig = XcodeBuildConfig;
        }

        public static iOSBuildSettings GetCurrentSettings()
        {
            return new iOSBuildSettings
            {
                BuildNumber = PlayerSettings.iOS.buildNumber,
                TargetOSVersion = PlayerSettings.iOS.targetOSVersionString,
                TargetDevice = PlayerSettings.iOS.targetDevice,
                SdkVersion = PlayerSettings.iOS.sdkVersion,
                AppleEnableAutomaticSigning = PlayerSettings.iOS.appleEnableAutomaticSigning,
                AppleDeveloperTeamID = PlayerSettings.iOS.appleDeveloperTeamID,
                ManualProvisioningProfileID = PlayerSettings.iOS.iOSManualProvisioningProfileID,
                ManualProvisioningProfileType = PlayerSettings.iOS.iOSManualProvisioningProfileType,
                XcodeBuildConfig = EditorUserBuildSettings.iOSXcodeBuildConfig,
            };
        }
    }
}