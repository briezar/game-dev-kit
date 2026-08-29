using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#if UNITY_WEBGL
using UnityEditor.WebGL;
#endif

namespace GameDevKit.Editor.AppBuild
{
    [Serializable]
    public class WebBuildConfig : BuildConfig
    {
        public WebBuildSettings WebBuildSettings;

        private WebBuildSettings _originalSettings;

        public WebBuildConfig()
        {
            BuildOptions = BuildOptions.Development;
            BuildTarget = BuildTarget.WebGL;
            BuildTargetGroup = BuildTargetGroup.WebGL;
            BuildSuffix = "-dev";

            WebBuildSettings = new()
            {
                ClientBrowserType = WebGLClientBrowserType.Default,
                TextureSubtarget = WebGLTextureSubtarget.Generic,
#if UNITY_WEBGL_API
                CodeOptimization = WasmCodeOptimization.BuildTimes,
#endif
                CompressionFormat = WebGLCompressionFormat.Gzip,
                NameFilesAsHashes = false,
                DataCaching = true,
                DebugSymbolMode = WebGLDebugSymbolMode.Off,
                ShowDiagnostics = false,
                DecompressionFallback = true,
                PowerPreference = WebGLPowerPreference.HighPerformance,
            };
        }

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

    }

    [Serializable]
    public class WebBuildSettings
    {
        public WebGLClientBrowserType ClientBrowserType;
        public WebGLTextureSubtarget TextureSubtarget;
#if UNITY_WEBGL
        public WasmCodeOptimization CodeOptimization;
#endif

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
#if UNITY_WEBGL
            UserBuildSettings.codeOptimization = CodeOptimization;
#endif

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
#if UNITY_WEBGL
                CodeOptimization = UserBuildSettings.codeOptimization,
#endif

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