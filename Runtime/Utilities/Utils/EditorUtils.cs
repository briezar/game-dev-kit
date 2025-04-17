#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace GameDevKit.Editor
{
    public static class EditorUtils
    {
        [MenuItem("Tools/Editor/Test")]
        private static void Test()
        {

        }

        [MenuItem("Tools/Editor/ForceReserializeAssets")]
        public static void ForceReserializeAssets()
        {
            if (EditorUtility.DisplayDialog("Force Reserialize Assets", "Are you sure you want to force reserialize all assets?", "Yes", "No"))
            {
                AssetDatabase.ForceReserializeAssets();
            }
        }

        [MenuItem("Tools/Editor/PrintCachePaths")]
        public static void PrintCachePaths()
        {
            var cachePaths = new List<string>();
            Caching.GetAllCachePaths(cachePaths);
            Debug.Log(cachePaths.JoinToString());

            if (cachePaths.Count == 1)
            {
                EditorUtility.RevealInFinder(cachePaths[0]);
            }
        }

        [MenuItem("Tools/Editor/ClearCache")]
        public static void ClearCache()
        {
            if (Caching.ClearCache())
            {
                Debug.Log("ClearCache success");
            }
            else
            {
                Debug.Log("ClearCache failed");
            }
        }

        [MenuItem("Tools/Editor/OpenApplication_DataPath")]
        public static void OpenApplication_DataPath()
        {
            EditorUtility.RevealInFinder(Application.dataPath);
            Debug.Log("Application.dataPath: " + Application.dataPath);
        }

        [MenuItem("Tools/Editor/OpenApplication_PersistentDataPath")]
        public static void OpenApplication_PersistentDataPath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
            Debug.Log("Application.dataPath: " + Application.persistentDataPath);
        }

        [MenuItem("Tools/Editor/PrintApplicationPaths")]
        public static void PrintApplicationPaths()
        {
            Debug.Log("Application.dataPath: " + Application.dataPath);
            Debug.Log("Application.persistentDataPath: " + Application.persistentDataPath);
        }

        [MenuItem("Tools/Set Dirty")]
        private static void SetDirty()
        {
            foreach (Object o in Selection.objects)
            {
                EditorUtility.SetDirty(o);
            }
        }

        [MenuItem("Tools/Editor/TestUniTask")]
        private static void TestUniTask()
        {
            if (!Application.isPlaying) { return; }

            var gameObject = new GameObject("Test");
            var token = gameObject.GetCancellationTokenOnDisable();
            Task();

            async UniTask Task()
            {
                Debug.Log("Task Start");
                while (true)
                {
                    var cancelled = await UniTask.WaitForSeconds(1, cancellationToken: token).SuppressCancellationThrow();
                    if (cancelled)
                    {
                        Debug.Log("Cancelled");
                        break;
                    }
                    Debug.Log(Time.time);
                }
                Debug.Log("Task End");
            }
        }

        public static void SetPrefabDirty(GameObject prefab)
        {
            var prefabStage = PrefabStageUtility.GetPrefabStage(prefab);
            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
        }

        public static Object[] FindAssets(string filter, string[] searchInFolders)
        {
            var guids = AssetDatabase.FindAssets($"{filter} a:assets", searchInFolders?.Where(s => !s.IsNullOrEmpty()).ToArray());

            var assets = guids.Select(guid =>
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                return asset;
            });

            return assets.ToArray();
        }

        public static Object[] FindAssets(Type type, params string[] searchInFolders)
        {
            var assets = FindAssets($"t:{type.Name}", searchInFolders);
            return assets.Where(a => a.GetType() == type).ToArray();
        }

        public static T[] FindAssets<T>(params string[] searchInFolders) where T : Object
        {
            var assets = FindAssets($"t:{typeof(T).Name}", searchInFolders);
            return assets.OfType<T>().ToArray();
        }

        public static T[] FindAssetsWithFilter<T>(string filter, params string[] searchInFolders) where T : Object
        {
            var assets = FindAssets($"{filter} t:{typeof(T).Name}", searchInFolders);
            return assets.OfType<T>().ToArray();
        }
    }
}

#endif