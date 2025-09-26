using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using GameDevKit.Editor;
#endif

namespace GameDevKit
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ScriptableObjectResourcesPathAttribute : Attribute
    {
        public readonly string RelativePath;
        public readonly string LoadPath;

        public ScriptableObjectResourcesPathAttribute(string relativePath)
        {
            if (!relativePath.Contains("/Resources/", StringComparison.Ordinal))
            {
                throw new InvalidPathException($"Path '{relativePath}' must contain a 'Resources' folder.");
            }

            var prefix = relativePath.StartsWith("Assets/", StringComparison.Ordinal) ? "Assets/" : string.Empty;
            var suffix = relativePath.EndsWith(".asset", StringComparison.Ordinal) ? string.Empty : ".asset";
            RelativePath = $"{prefix}{relativePath}{suffix}";

            LoadPath = RelativePath.Split("/Resources/")[^1].Replace(".asset", string.Empty);
        }

        public class InvalidPathException : Exception
        {
            public InvalidPathException(string message) : base(message) { }
        }
    }

    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;
        protected static T instance
        {
            get
            {
                if (_instance == null)
                {
                    var pathAttribute = GetPathAttribute();

                    _instance = Resources.Load<T>(pathAttribute.LoadPath);
                    if (_instance == null)
                    {
#if UNITY_EDITOR
                        var infos = EditorUtils.FindAssets<T>();
                        if (infos.Length == 0)
                        {
                            _instance = CreateInstance<T>();
                            AssetDatabase.CreateAsset(_instance, pathAttribute.RelativePath);
                            Debug.Log($"Created new {typeof(T).Name} at {pathAttribute.RelativePath}", _instance);
                        }
                        else if (infos.Length >= 1)
                        {
                            _instance = infos[0];
                            AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(_instance), pathAttribute.RelativePath);
                            Debug.LogWarning($"Moved {_instance} to {pathAttribute.RelativePath}", _instance);
                            if (infos.Length > 1)
                            {
                                Debug.LogWarning($"Multiple {typeof(T).Name} found! Using the first one: {_instance.name}", _instance);
                            }
                        }
#endif
                    }
                }
                return _instance;
            }
        }

        protected SingletonScriptableObject()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogError($"Another instance of {typeof(T).Name} already exists: {_instance.name}", this);
                return;
            }

#if UNITY_EDITOR
            try
            {
                // Validate the attribute
                GetPathAttribute();
            }
            catch (ScriptableObjectResourcesPathAttribute.InvalidPathException ex)
            {
                Debug.LogError($"Invalid Path for {typeof(T).Name}\n{ex.Message}", this);
                return;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex, this);
                return;
            }
#endif
        }

        protected static ScriptableObjectResourcesPathAttribute GetPathAttribute()
        {
            var type = typeof(T);
            var pathAttribute = type.GetCustomAttribute<ScriptableObjectResourcesPathAttribute>();
            if (pathAttribute == null)
            {
                Debug.LogError($"Class {type.Name} must have a {nameof(ScriptableObjectResourcesPathAttribute)}.");
                return null;
            }
            return pathAttribute;
        }


        [Conditional("UNITY_EDITOR")]
        public new static void SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(instance);
#endif
        }
    }
}