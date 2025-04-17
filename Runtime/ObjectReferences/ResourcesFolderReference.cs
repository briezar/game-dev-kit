using System;
using UnityEngine;
using System.Linq;

namespace GameDevKit.ObjectReferences
{
    /// <summary>
    /// A wrapper to serialize Folder References.
    /// </summary>
    [Serializable]
    public class ResourcesFolderReference : FolderReference
    {
#if UNITY_EDITOR
        public override bool IsAssetValid => base.IsAssetValid && IsInResourcesFolder();

        protected override void HandleBeforeSerialize()
        {
            base.HandleBeforeSerialize();
            _folderPath = _folderPath.Split("Resources/").Last();
        }
        private bool IsInResourcesFolder() => GetAssetPath().Contains("Resources/");
#endif
    }
}