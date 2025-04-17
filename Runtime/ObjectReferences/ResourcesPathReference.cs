using System;
using UnityEngine;
using System.Linq;
using System.IO;

namespace GameDevKit.ObjectReferences
{
    /// <summary>
    /// Serialize Asset Path in Resources folder as string.
    /// </summary>
    [Serializable]
    public class ResourcesPathReference : PathReference
    {
#if UNITY_EDITOR
        public override bool IsAssetValid => base.IsAssetValid && IsInResourcesFolder();

        private bool IsInResourcesFolder() => GetAssetPath().Contains("Resources/");

        protected override void HandleBeforeSerialize()
        {
            if (_asset != null && !IsInResourcesFolder())
            {
                Debug.LogError($"{_asset.name} is not in Resources folder!");
            }
            base.HandleBeforeSerialize();
            _path = Path.GetFileNameWithoutExtension(_path).Split("Resources/").Last();
        }
#endif
    }
}