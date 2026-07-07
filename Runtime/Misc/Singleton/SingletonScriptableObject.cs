using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Reflection;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using GameDevKit.Editor;
#endif

namespace GameDevKit
{
    /// <summary>
    /// Attribute to specify the relative path (from Assets folder) of a ScriptableObject singleton that inherits from <see cref="SingletonScriptableObject{T}"/>.<br/>
    /// The asset will be loaded from the Resources folder at runtime, and created/moved there in the editor if it doesn't exist or is found elsewhere.
    /// The path must contain a "Resources" folder and end with the asset name. For example: "[Assets/]_Project/Resources/Singletons/MySingleton[.asset]".
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ScriptableObjectResourcesPathAttribute : Attribute
    {
        public readonly string RelativePath;
        public readonly string LoadPath;
        public readonly string AbsolutePath;
        public readonly string AssetName;

        public ScriptableObjectResourcesPathAttribute(string relativePath)
        {
            if (!relativePath.Contains("/Resources/", StringComparison.Ordinal))
            {
                throw new InvalidPathException($"Path '{relativePath}' must contain a 'Resources' folder.");
            }

            var prefix = relativePath.StartsWith("Assets/", StringComparison.Ordinal) ? string.Empty : "Assets/";
            var suffix = relativePath.EndsWith(".asset", StringComparison.Ordinal) ? string.Empty : ".asset";
            RelativePath = $"{prefix}{relativePath}{suffix}";
            LoadPath = RelativePath.Split("/Resources/")[^1].Replace(".asset", string.Empty);
            AbsolutePath = Directory.GetParent(Application.dataPath).FullName + "/" + RelativePath;

            AssetName = Path.GetFileNameWithoutExtension(RelativePath);
        }

        public class InvalidPathException : Exception
        {
            public InvalidPathException(string message) : base(message) { }
        }
    }

    /// <summary>
    /// Base class for ScriptableObject singletons loaded from Resources folder.  Must have <see cref="ScriptableObjectResourcesPathAttribute"/> defined.
    /// </summary>
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
                            try
                            {
                                Directory.CreateDirectory(Directory.GetParent(pathAttribute.AbsolutePath).FullName);
                                var oldPath = Directory.GetParent(Application.dataPath).FullName + "/" + AssetDatabase.GetAssetPath(_instance);
                                File.Move(oldPath, pathAttribute.AbsolutePath);
                                File.Move($"{oldPath}.meta", $"{pathAttribute.AbsolutePath}.meta");

                                Debug.LogWarning($"Moved {_instance} to {pathAttribute.RelativePath}", _instance);

                                AssetDatabase.Refresh();
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError(ex);
                            }

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