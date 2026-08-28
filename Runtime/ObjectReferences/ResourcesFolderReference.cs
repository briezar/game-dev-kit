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
        protected override bool IsAssetValid => base.IsAssetValid && IsInResourcesFolder();

        protected override void HandleBeforeSerialize()
        {
            if (_folderAsset == null)
            {
                _folderPath = string.Empty;
                return;
            }

            if (!IsAssetValid)
            {
                Debug.LogError($"{_folderAsset.name} must be folder inside any Resources folder!");

                // Try recover from path
                _folderAsset = GetFolderAsset();
                if (_folderAsset == null)
                {
                    _folderPath = string.Empty;
                }

                return;
            }

            _folderPath = GetAssetPath();

            UnityEditor.EditorUtility.SetDirty(_folderAsset);
        }
        private bool IsInResourcesFolder() => GetAssetPath().Contains("Resources/");
#endif

        public string ResourcesPath => _folderPath.Split("Resources/").Last();
    }
}