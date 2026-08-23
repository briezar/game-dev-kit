using System;
using System.IO;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using GameDevKit.Attributes;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.AppBuild
{
    [CreateAssetMenu(menuName = EditorConstants.MenuPath + "AppBuild/BuildConfig")]
    public class BuildConfigSO : ScriptableObject
    {
        [SerializeReference, SubclassPicker]
        public BuildConfig BuildConfig;

        [ShowInInspector]
        private string _buildPath => BuildConfig?.GetBuildPath() ?? "N/A";

        [Button]
        public async UniTask Build()
        {
            if (BuildConfig == null)
            {
                Debug.LogError("BuildConfig is null. Please assign a BuildConfig before building.", this);
                return;
            }

            if (!EditorUtility.DisplayDialog("Confirm Build", $"Are you sure you want to build?", "Yes", "No")) { return; }

            var buildPath = BuildConfig.GetBuildPath();
            Debug.Log($"Building {name} with config: {BuildConfig.ToJsonUnity()}", this);
            await BuildConfig.PreBuildAsync();
            foreach (var addon in BuildConfig.AddOns)
            {
                await addon.PreBuildAsync();
            }
            SaveAsset();

            var buildPlayerOptions = BuildConfig.GetBuildPlayerOptions();
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log($"Finished building {name}", this);

            await BuildConfig.PostBuildAsync();
            foreach (var addon in BuildConfig.AddOns)
            {
                await addon.PostBuildAsync();
            }
            SaveAsset();

            EditorUtility.RevealInFinder(buildPath);
        }

        private void SaveAsset()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [Button]
        public void OpenBuildFolder()
        {
            if (BuildConfig == null)
            {
                Debug.LogError("BuildConfig is null. Please assign a BuildConfig before opening build folder.", this);
                return;
            }

            var buildPath = BuildConfig.GetBuildPath();
            if (string.IsNullOrEmpty(buildPath))
            {
                Debug.LogError("Build path is null or empty. Cannot open build folder.", this);
                return;
            }

            var directoryInfo = Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", buildPath));
            Debug.Log(directoryInfo.FullName);

            EditorUtility.RevealInFinder(buildPath);
        }

    }
}
