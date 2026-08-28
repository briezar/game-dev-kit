using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameDevKit.ObjectReferences
{
    /// <summary>
    /// A wrapper to serialize Folder References.
    /// </summary>
    [Serializable]
    public class FolderReference : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [SerializeField] protected UnityEngine.Object _folderAsset;

        protected virtual bool IsAssetValid => IsFolder;
        protected bool IsFolder => _folderAsset != null && AssetDatabase.IsValidFolder(GetAssetPath());
#endif

        // This should only ever be set during serialization/deserialization!
        [SerializeField]
        [HideInInspector]
        protected string _folderPath = string.Empty;

        public string FolderPath => _folderPath;

        public static implicit operator string(FolderReference folderRef)
        {
            return folderRef.FolderPath;
        }

        // Called to prepare this data for serialization. Stubbed out when not in editor.
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            HandleBeforeSerialize();
#endif
        }

        // Called to set up data for deserialization. Stubbed out when not in editor.
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            // We cannot touch AssetDatabase during serialization, so defer by a bit.
            EditorApplication.update += HandleAfterDeserialize;
#endif
        }

#if UNITY_EDITOR
        protected UnityEngine.Object GetFolderAsset()
        {
            return string.IsNullOrEmpty(_folderPath) ? null : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_folderPath);
        }

        protected string GetAssetPath()
        {
            return _folderAsset == null ? string.Empty : AssetDatabase.GetAssetPath(_folderAsset);
        }

        protected virtual void HandleBeforeSerialize()
        {
            if (_folderAsset == null)
            {
                _folderPath = string.Empty;
                return;
            }

            if (!IsAssetValid)
            {
                Debug.LogError($"{_folderAsset.name} must be a folder!");

                // Try recover from path
                _folderAsset = GetFolderAsset();
                if (_folderAsset == null)
                {
                    _folderPath = string.Empty;
                }

                return;
            }

            _folderPath = GetAssetPath();

            EditorUtility.SetDirty(_folderAsset);
        }

        private void HandleAfterDeserialize()
        {
            EditorApplication.update -= HandleAfterDeserialize;
            // Asset is valid, don't do anything - Path will always be set based on it when it matters
            if (!IsFolder) { return; }

            if (string.IsNullOrEmpty(_folderPath)) { return; }

            // Asset is invalid, try recover from path
            _folderAsset = GetFolderAsset();
            if (_folderAsset == null) { _folderPath = string.Empty; }

            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(_folderAsset);
            }
        }
#endif

        public override string ToString()
        {
            return _folderPath;
        }
    }
}