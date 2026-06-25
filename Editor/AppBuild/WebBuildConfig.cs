using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.AppBuild
{
    [Serializable]
    public class WebBuildConfig : BuildConfig
    {
        public WebBuildSettings WebBuildSettings;

        private WebBuildSettings _originalSettings;

        public override async UniTask PreBuildAsync()
        {
            _originalSettings = WebBuildSettings.GetCurrentSettings();
            WebBuildSettings.Apply();
            Debug.Log($"Applied Web build settings:\n{WebBuildSettings.ToJsonUnity()}");
            await UniTask.SwitchToMainThread();
        }

        public override async UniTask PostBuildAsync()
        {
            _originalSettings.Apply();
            Debug.Log($"Restored original Web build settings:\n{_originalSettings.ToJsonUnity()}");
            await UniTask.SwitchToMainThread();
        }

        public override BuildPlayerOptions GetBuildPlayerOptions()
        {
            WebBuildSettings.Apply();
            return base.GetBuildPlayerOptions();
        }
    }

    [Serializable]
    public class WebBuildSettings
    {
        public WebGLClientBrowserType ClientBrowserType;
        public WebGLTextureSubtarget TextureSubtarget;

        public WebGLCompressionFormat CompressionFormat;
        public bool NameFilesAsHashes;
        public bool DataCaching = true;
        public WebGLDebugSymbolMode DebugSymbolMode;
        public bool ShowDiagnostics;
        public bool DecompressionFallback;
        public WebGLPowerPreference PowerPreference = WebGLPowerPreference.HighPerformance;

        public void Apply()
        {
            EditorUserBuildSettings.webGLClientBrowserType = ClientBrowserType;
            EditorUserBuildSettings.webGLBuildSubtarget = TextureSubtarget;

            PlayerSettings.WebGL.compressionFormat = CompressionFormat;
            PlayerSettings.WebGL.nameFilesAsHashes = NameFilesAsHashes;
            PlayerSettings.WebGL.dataCaching = DataCaching;
            PlayerSettings.WebGL.debugSymbolMode = DebugSymbolMode;
            PlayerSettings.WebGL.showDiagnostics = ShowDiagnostics;
            PlayerSettings.WebGL.decompressionFallback = DecompressionFallback;
            PlayerSettings.WebGL.powerPreference = PowerPreference;
        }

        public static WebBuildSettings GetCurrentSettings()
        {
            return new WebBuildSettings
            {
                ClientBrowserType = EditorUserBuildSettings.webGLClientBrowserType,
                TextureSubtarget = EditorUserBuildSettings.webGLBuildSubtarget,

                CompressionFormat = PlayerSettings.WebGL.compressionFormat,
                NameFilesAsHashes = PlayerSettings.WebGL.nameFilesAsHashes,
                DataCaching = PlayerSettings.WebGL.dataCaching,
                DebugSymbolMode = PlayerSettings.WebGL.debugSymbolMode,
                ShowDiagnostics = PlayerSettings.WebGL.showDiagnostics,
                DecompressionFallback = PlayerSettings.WebGL.decompressionFallback,
                PowerPreference = PlayerSettings.WebGL.powerPreference,
            };
        }
    }
}