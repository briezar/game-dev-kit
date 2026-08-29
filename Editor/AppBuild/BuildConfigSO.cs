using System;
using System.Collections.Generic;
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

        [SerializeReference, SubclassPicker]
        public List<BuildConfigAddOn> AddOns;

        [ShowInInspector]
        private string BuildPath => BuildConfig?.GetBuildPath() ?? "N/A";

        [Button]
        public async UniTask Build()
        {
            if (BuildConfig == null)
            {
                Debug.LogError("BuildConfig is null. Please assign a BuildConfig before building.", this);
                return;
            }

            if (!EditorUtility.DisplayDialog("Confirm Build", $"Are you sure you want to build?", "Yes", "No")) { return; }

            Debug.Log($"Building {name} with config: {BuildConfig.ToJsonUnity()}", this);
            await BuildConfig.PreBuildAsync();
            foreach (var addon in AddOns)
            {
                addon.BuildConfig = BuildConfig;
                await addon.PreBuildAsync();
            }
            SaveAsset();

            var buildPlayerOptions = BuildConfig.GetBuildPlayerOptions();
            var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log($"Finished building {name}. Result: {buildReport.summary.result}", this);

            await BuildConfig.PostBuildAsync(buildReport);
            foreach (var addon in AddOns)
            {
                await addon.PostBuildAsync(buildReport);
            }
            SaveAsset();

            if (buildReport.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                EditorUtility.RevealInFinder(BuildPath);
            }
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

            var buildDirectory = Path.GetDirectoryName(Path.GetFullPath(buildPath));
            if (!Directory.Exists(buildDirectory))
            {
                Directory.CreateDirectory(buildDirectory);
                Debug.Log($"Created directory: {buildDirectory}");
            }

            EditorUtility.RevealInFinder(buildPath);
        }

    }
}
