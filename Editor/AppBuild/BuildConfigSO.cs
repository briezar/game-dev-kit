using System;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using GameDevKit.Attributes;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.AppBuild
{
    [CreateAssetMenu(menuName = "AppBuild/BuildConfig", order = 0)]
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

            var buildPlayerOptions = BuildConfig.GetBuildPlayerOptions();
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log($"Finished building {name}", this);

            await BuildConfig.PostBuildAsync();

            EditorUtility.RevealInFinder(buildPath);
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

            EditorUtility.RevealInFinder(buildPath);
        }

    }
}
