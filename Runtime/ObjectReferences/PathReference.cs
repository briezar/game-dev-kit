using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameDevKit.ObjectReferences
{
    /// <summary>
    /// Serialize Asset Path as string.
    /// </summary>
    [Serializable]
    public class PathReference : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [SerializeField] protected UnityEngine.Object _asset;

        public virtual bool IsAssetValid
        {
            get
            {
                return _asset != null;
            }
        }
#endif

        // This should only ever be set during serialization/deserialization!
        [SerializeField]
        [HideInInspector]
        protected string _path = string.Empty;

        public string path => _path;

        public static implicit operator string(PathReference pathRef)
        {
            return pathRef.path;
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
        protected UnityEngine.Object GetAsset()
        {
            return string.IsNullOrEmpty(_path) ? null : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_path);
        }

        protected string GetAssetPath()
        {
            return _asset == null ? string.Empty : AssetDatabase.GetAssetPath(_asset);
        }

        protected virtual void HandleBeforeSerialize()
        {
            var isValid = IsAssetValid;

            if (isValid)
            {
                _path = GetAssetPath();
                return;
            }

            if (_asset == null)
            {
                _path = string.Empty;
                return;
            }

            // Try recover from path
            _asset = GetAsset();
            if (_asset == null)
            {
                _path = string.Empty;
                return;
            }

            EditorUtility.SetDirty(_asset);

        }

        private void HandleAfterDeserialize()
        {
            EditorApplication.update -= HandleAfterDeserialize;
            // Asset is valid, don't do anything - Path will always be set based on it when it matters
            if (IsAssetValid) { return; }

            if (string.IsNullOrEmpty(_path)) { return; }

            // Asset is invalid, try recover from path
            _asset = GetAsset();
            if (_asset == null) { _path = string.Empty; }

            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(_asset);
            }
        }
#endif

        public override string ToString()
        {
            return _path;
        }
    }
}