using GameDevKit.ObjectReferences;
using UnityEngine;
using EditorAttributes;
using AYellowpaper.SerializedCollections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameDevKit.UI
{
    [CreateAssetMenu(menuName = MenuName)]
    public class UIResourceMapSO : ScriptableObject
    {
        [HelpBox("This will be auto-filled.", drawAbove: true)]
        [SerializeField] private SerializedDictionary<string, string> _uiResourceMap;

#if UNITY_EDITOR
        [HelpBox("The Resources folder that holds all your UI. Folder structure doesn't matter, just need to be inside a Resources folder.", drawAbove: true)]
        [SerializeField] private ResourcesFolderReference _uiFolder;

        public const string MenuName = "GameDevKit/UI/UIResourceMap";
        private static UIResourceMapSO _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void GenerateOnLoad() => _instance?.GenerateResourcePaths();
        private void OnEnable() => _instance = this;

        [Button]
        private void GenerateResourcePaths()
        {
            if (_uiFolder == null)
            {
                Debug.LogError($"UIResourceMap: UI Folder is not set. Please assign a folder that holds all of your UI in the inspector.", this);
                return;
            }

            var before = _uiResourceMap.ToJson();

            _uiResourceMap.Clear();

            var guids = AssetDatabase.FindAssets($"t:{nameof(GameObject)} a:assets", new[] { _uiFolder.FolderPath });
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (!asset.TryGetComponent<UIView>(out var view))
                {
                    Debug.LogWarning($"{asset.name} does not have UIView component, skipping.", asset);
                    continue;
                }

                var fullName = view.GetType().FullName;
                var resourcePath = assetPath.Split("Resources/")[1].Replace(".prefab", "");
                _uiResourceMap[fullName] = resourcePath;
            }

            var after = _uiResourceMap.ToJson();
            if (before.FastOrdinalEquals(after)) { return; }

            EditorUtility.SetDirty(this);

            Debug.Log($"Updated UI resource paths from {_uiFolder}");
        }
#endif

        public T LoadResource<T>() where T : UIView, new()
        {
            var resourcePath = GetResourcePath<T>();
            if (resourcePath == null) { return null; }

            var resource = Resources.Load<T>(resourcePath);
            if (resource == null)
            {
                Debug.LogError($"UIResourceMap: Resource not found for {typeof(T).Name} at path {resourcePath}");
                return null;
            }
            return resource;
        }

        public string GetResourcePath<T>() where T : UIView, new()
        {
            var typeName = typeof(T).FullName;
            if (_uiResourceMap.TryGetValue(typeName, out var path))
            {
                return path;
            }

            Debug.LogError($"UIResourceMap: Resource path not found for {typeName}");
            return null;
        }
    }
}