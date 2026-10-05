using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.IO;
using System.Linq;

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

        /// <summary>
        /// True if this attribute was generated from the type name because the type has none defined.
        /// </summary>
        public readonly bool UseDefaultPath;

        public ScriptableObjectResourcesPathAttribute(string relativePath) : this(relativePath, false) { }

        private ScriptableObjectResourcesPathAttribute(string relativePath, bool useDefaultPath)
        {
            if (!relativePath.Contains("/Resources/", StringComparison.Ordinal))
            {
                throw new InvalidPathException($"Path '{relativePath}' must contain a 'Resources' folder.");
            }

            var prefix = relativePath.StartsWith("Assets/", StringComparison.Ordinal) ? string.Empty : "Assets/";
            var suffix = relativePath.EndsWith(".asset", StringComparison.Ordinal) ? string.Empty : ".asset";
            RelativePath = $"{prefix}{relativePath}{suffix}";
            LoadPath = RelativePath.Split("/Resources/")[^1].RemoveFromEnd(".asset");
            AbsolutePath = Directory.GetParent(Application.dataPath).FullName + "/" + RelativePath;

            AssetName = Path.GetFileNameWithoutExtension(RelativePath);
            UseDefaultPath = useDefaultPath;
        }

        /// <summary>
        /// Returns the attribute defined on the type, or one using "Assets/Resources/{Type.FullName without SO suffix}.asset" if the type has none.
        /// </summary>
        public static ScriptableObjectResourcesPathAttribute Resolve(Type type)
        {
            var attribute = type.GetCustomAttribute<ScriptableObjectResourcesPathAttribute>();
            return attribute ?? new ScriptableObjectResourcesPathAttribute(GetDefaultPath(type), true);
        }

        private static string GetDefaultPath(Type type) => $"Assets/Resources/{type.FullName.RemoveFromEnd("SO")}.asset";

#if UNITY_EDITOR
        /// <summary>
        /// Logs a warning if this attribute was generated from the type name, recommending to define it explicitly.
        /// </summary>
        public void WarnIfDefault(Type type)
        {
            if (!UseDefaultPath) { return; }

            Debug.LogWarning($"Class {type.Name} has no {nameof(ScriptableObjectResourcesPathAttribute)}. Using default path '{RelativePath}'. It is recommended to define the attribute explicitly.");
        }

        /// <summary>
        /// Creates the directory of the defined path if missing. Returns true if it had to be created.
        /// </summary>
        public bool EnsureDirectory()
        {
            var directory = Directory.GetParent(AbsolutePath).FullName;
            if (Directory.Exists(directory)) { return false; }

            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
            return true;
        }

        /// <summary>
        /// Moves the asset and its .meta file to the defined path, creating the matching directories, then refreshes the AssetDatabase.
        /// </summary>
        public bool MoveAssetPathToAttributePath(string currentAssetPath)
        {
            if (currentAssetPath == RelativePath) { return true; }

            if (File.Exists(AbsolutePath))
            {
                Debug.LogError($"Cannot move '{currentAssetPath}' to '{RelativePath}': a file already exists there.");
                return false;
            }

            try
            {
                var currentAbsolutePath = Directory.GetParent(Application.dataPath).FullName + "/" + currentAssetPath;
                EnsureDirectory();
                File.Move(currentAbsolutePath, AbsolutePath);

                var currentMetaPath = $"{currentAbsolutePath}.meta";
                if (File.Exists(currentMetaPath))
                {
                    File.Move(currentMetaPath, $"{AbsolutePath}.meta");
                }

                Debug.LogWarning($"Moved '{currentAssetPath}' to '{RelativePath}'.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return false;
            }
            finally
            {
                AssetDatabase.Refresh();
            }
        }
#endif

        public class InvalidPathException : Exception
        {
            public InvalidPathException(string message) : base(message) { }
        }
    }

    /// <summary>
    /// Base class for ScriptableObject singletons loaded from Resources folder.  Should have <see cref="ScriptableObjectResourcesPathAttribute"/> defined, otherwise "Assets/Resources/{Type.FullName without SO suffix}.asset" is used.<br/>
    /// In the editor, newly created assets of a singleton type are deleted if an instance already exists, otherwise they are moved to the defined path.
    /// </summary>
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
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
                            pathAttribute.EnsureDirectory();
                            _instance = CreateInstance<T>();
                            AssetDatabase.CreateAsset(_instance, pathAttribute.RelativePath);
                            Debug.Log($"Created new {typeof(T).Name} at {pathAttribute.RelativePath}", _instance);
                            pathAttribute.WarnIfDefault(typeof(T));
                        }
                        else
                        {
                            _instance = infos[0];
                            pathAttribute.MoveAssetPathToAttributePath(AssetDatabase.GetAssetPath(_instance));

                            if (infos.Length > 1)
                            {
                                Debug.LogWarning($"Multiple {typeof(T).Name} found! Using the first one: {_instance.name}", _instance);
                            }
                        }
#else
                        throw new InvalidOperationException($"No instance of {typeof(T).Name} found and unable to create one outside of the editor.");
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

        protected static ScriptableObjectResourcesPathAttribute GetPathAttribute() => ScriptableObjectResourcesPathAttribute.Resolve(typeof(T));
    }

#if UNITY_EDITOR
    /// <summary>
    /// Detects newly created assets and, for <see cref="SingletonScriptableObject{T}"/> types, deletes them if an instance already exists or moves them to their defined path.<br/>
    /// Also blocks moving or renaming a singleton asset away from its defined path.
    /// </summary>
    internal class SingletonScriptableObjectAssetProcessor : AssetModificationProcessor
    {
        private static readonly HashSet<string> _pendingPaths = new();

        private static void OnWillCreateAsset(string path)
        {
            if (!path.EndsWith(".asset", StringComparison.Ordinal)) { return; }

            if (_pendingPaths.Add(path) && _pendingPaths.Count == 1)
            {
                EditorApplication.delayCall += ProcessPendingPaths;
            }
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (!AssetDatabase.IsValidFolder(sourcePath))
            {
                return IsMoveAllowed(sourcePath, destinationPath) ? AssetMoveResult.DidNotMove : AssetMoveResult.FailedMove;
            }

            // A folder move only reports the folder, so check every singleton asset inside it against where it would end up.
            foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { sourcePath }))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var newAssetPath = destinationPath + assetPath[sourcePath.Length..];
                if (!IsMoveAllowed(assetPath, newAssetPath))
                {
                    return AssetMoveResult.FailedMove;
                }
            }
            return AssetMoveResult.DidNotMove;
        }

        private static bool IsMoveAllowed(string sourcePath, string destinationPath)
        {
            var type = AssetDatabase.GetMainAssetTypeAtPath(sourcePath);
            if (!type.Implements(typeof(SingletonScriptableObject<>))) { return true; }

            if (!TryResolvePathAttribute(type, out var pathAttribute)) { return true; }

            // Allow moving a misplaced asset to its defined path.
            if (destinationPath == pathAttribute.RelativePath) { return true; }

            Debug.LogWarning($"{type.Name} must stay at '{pathAttribute.RelativePath}'. Blocked moving '{sourcePath}' to '{destinationPath}'.");
            return false;
        }

        private static void ProcessPendingPaths()
        {
            var paths = _pendingPaths.ToArray();
            _pendingPaths.Clear();

            foreach (var path in paths)
            {
                HandleCreatedAsset(path);
            }
        }

        private static void HandleCreatedAsset(string assetPath)
        {
            var type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (!type.Implements(typeof(SingletonScriptableObject<>))) { return; }

            if (!TryResolvePathAttribute(type, out var pathAttribute)) { return; }

            // Already at the defined path, e.g. created by the singleton getter.
            if (assetPath == pathAttribute.RelativePath) { return; }

            if (AssetDatabase.GetMainAssetTypeAtPath(pathAttribute.RelativePath) == type)
            {
                Debug.LogError($"A {type.Name} already exists at '{pathAttribute.RelativePath}'. Deleted the new asset at '{assetPath}'.", AssetDatabase.LoadMainAssetAtPath(pathAttribute.RelativePath));
                AssetDatabase.DeleteAsset(assetPath);
                return;
            }

            if (pathAttribute.MoveAssetPathToAttributePath(assetPath))
            {
                pathAttribute.WarnIfDefault(type);
            }
        }

        private static bool TryResolvePathAttribute(Type type, out ScriptableObjectResourcesPathAttribute pathAttribute)
        {
            try
            {
                pathAttribute = ScriptableObjectResourcesPathAttribute.Resolve(type);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                pathAttribute = null;
                return false;
            }
        }
    }
#endif
}