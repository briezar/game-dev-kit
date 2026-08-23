using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.AppBuild
{
    [Serializable]
    public abstract class BuildConfigAddOn
    {
        public virtual UniTask PreBuildAsync() => UniTask.CompletedTask;
        public virtual UniTask PostBuildAsync() => UniTask.CompletedTask;
    }
}