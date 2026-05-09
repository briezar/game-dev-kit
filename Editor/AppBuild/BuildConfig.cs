using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.AppBuild
{
    public abstract class BuildConfig
    {
        public string BuildFolder = "Builds";
        public string[] ExtraScriptingDefines;
        public BuildOptions BuildOptions;
        public BuildTarget BuildTarget;
        public BuildTargetGroup BuildTargetGroup;
        public string BuildSuffix;

        public virtual UniTask PreBuildAsync() => UniTask.CompletedTask;
        public virtual UniTask PostBuildAsync() => UniTask.CompletedTask;

        public abstract BuildPlayerOptions GetBuildPlayerOptions();

        public abstract string GetExtension();
        public virtual string GetBuildPath() => $"{BuildFolder}/{BuildTarget}/{Application.productName}{BuildSuffix}.{GetExtension()}";

        protected static string[] GetScenes() => EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
    }

}
