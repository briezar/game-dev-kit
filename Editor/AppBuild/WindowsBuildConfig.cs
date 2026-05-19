using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.AppBuild
{
    [Serializable]
    public class WindowsBuildConfig : BuildConfig
    {
        public WindowsBuildSettings WindowsBuildSettings;

        private WindowsBuildSettings _originalSettings;

        public override async UniTask PreBuildAsync()
        {
            _originalSettings = WindowsBuildSettings.GetCurrentSettings();
            WindowsBuildSettings.Apply();
            Debug.Log($"Applied Windows build settings:\n{WindowsBuildSettings.ToJsonUnity()}");
            await UniTask.SwitchToMainThread();
        }

        public override async UniTask PostBuildAsync()
        {
            _originalSettings.Apply();
            Debug.Log($"Restored original Windows build settings:\n{_originalSettings.ToJsonUnity()}");
            await UniTask.SwitchToMainThread();
        }

        public override BuildPlayerOptions GetBuildPlayerOptions()
        {
            WindowsBuildSettings.Apply();
            return base.GetBuildPlayerOptions();
        }

    }

    [Serializable]
    public class WindowsBuildSettings
    {
        public StandaloneBuildSubtarget BuildSubtarget;

        public void Apply()
        {
            EditorUserBuildSettings.standaloneBuildSubtarget = BuildSubtarget;
        }

        public static WindowsBuildSettings GetCurrentSettings()
        {
            return new WindowsBuildSettings
            {
                BuildSubtarget = EditorUserBuildSettings.standaloneBuildSubtarget,
            };
        }
    }
}