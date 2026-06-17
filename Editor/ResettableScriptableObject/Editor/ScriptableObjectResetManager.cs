using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace GameDevKit.Editor
{
    public class ScriptableObjectResetManager : ScriptableSingleton<ScriptableObjectResetManager>
    {
        public List<string> Guids = new(); // GUIDs of resettable ScriptableObjects, used to avoid loading all assets in the project
        public List<SerializableType> ResettableTypes = new();

        public static readonly List<Type> CachedResettableTypes = TypeCache.GetTypesWithAttribute<ResetOnExitPlayModeAttribute>().ToList();

        private void Awake()
        {
            UpdateResettableTypes();
            PopulateGuids();
        }

        public void PopulateGuids()
        {
            Guids.Clear();
            foreach (var type in ResettableTypes)
            {
                var guids = EditorUtils.FindAssetGuids(type, "");
                foreach (var guid in guids)
                {
                    if (Guids.Contains(guid, StringComparer.Ordinal)) { continue; }
                    Guids.Add(guid);
                }
            }
            if (Guids.Count == 0) { return; }

            var assetNames = Guids.Select(guid => AssetPathToName(AssetDatabase.GUIDToAssetPath(guid)));
            Debug.Log($"Registered {Guids.Count} resettable {nameof(ScriptableObject)}(s): {assetNames.JoinToString()}");
        }

        private static string AssetPathToName(string path) => path.Split('/')[^1].Replace(".asset", "", StringComparison.Ordinal);

        public bool TryAddAssetAtPath(string path)
        {
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (Guids.Contains(guid, StringComparer.Ordinal)) { return false; }

            var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (obj == null) { return false; }
            if (!obj.HasAttribute<ResetOnExitPlayModeAttribute>()) { return false; }

            Guids.Add(guid);
            Debug.Log($"Added resettable {nameof(ScriptableObject)}: {obj}");
            return true;
        }

        public bool TryUpdateResettableTypes()
        {
            if (CachedResettableTypes.Count == 0) { return false; }

            for (int i = ResettableTypes.Count - 1; i >= 0; i--)
            {
                var type = ResettableTypes[i];
                if (!type.IsValid)
                {
                    ResettableTypes.RemoveAt(i);
                    Debug.LogWarning($"Type [{type.AssemblyQualifiedName}] is no longer valid, removing from resettable types.");
                    continue;
                }
            }

            var currentTypes = CachedResettableTypes.Select(t => new SerializableType(t)).ToArray();
            var uniqueElements = ResettableTypes.SymmetricExcept(currentTypes).ToArray();
            if (uniqueElements.Length == 0) { return false; }

            Debug.Log($"Change in resettable {nameof(ScriptableObject)} types detected. Changed types: {uniqueElements.JoinToString(e => e.Type.Name)}");
            UpdateResettableTypes();
            PopulateGuids();
            return true;
        }

        public void UpdateResettableTypes()
        {
            ResettableTypes.Clear();
            foreach (var type in CachedResettableTypes)
            {
                if (!type.Implements<ScriptableObject>())
                {
                    Debug.LogWarning($"Type {type.FullName} is marked with {nameof(ResetOnExitPlayModeAttribute)} but does not inherit from {nameof(ScriptableObject)}.");
                    continue;
                }
                ResettableTypes.Add(type);
            }
        }

        public IEnumerable<ScriptableObject> EnumerateResettableObjects()
        {
            foreach (var guid in Guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (obj != null)
                {
                    yield return obj;
                }
            }
        }
    }

    public class ScriptableObjectResetManagerPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (ScriptableObjectResetManager.CachedResettableTypes.Count == 0)
            {
                return;
            }

            if (didDomainReload)
            {
                ScriptableObjectResetManager.instance.TryUpdateResettableTypes();
                return;
            }

            // Check new or changed assets
            foreach (var path in importedAssets)
            {
                if (!path.EndsWith(".asset", StringComparison.Ordinal)) { continue; }
                ScriptableObjectResetManager.instance.TryAddAssetAtPath(path);
            }

            // Remove deleted assets
            foreach (var path in deletedAssets)
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                ScriptableObjectResetManager.instance.Guids.Remove(guid);
            }
        }
    }

}