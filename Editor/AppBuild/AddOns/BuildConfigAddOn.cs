using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GameDevKit.Editor.AppBuild
{
    [Serializable]
    public abstract class BuildConfigAddOn
    {
        public BuildConfig BuildConfig { get; internal set; }

        public virtual UniTask PreBuildAsync() => UniTask.CompletedTask;
        public virtual UniTask PostBuildAsync(BuildReport buildReport) => UniTask.CompletedTask;
    }
}