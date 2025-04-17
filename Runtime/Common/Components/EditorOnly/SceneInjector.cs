#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GameDevKit.Editor
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    public class SceneInjector : MonoBehaviour
    {
        private const string EditorOnlyTag = "EditorOnly";
        private static readonly HashSet<string> _loadedScenePaths = new();

        [SerializeField] private SceneAsset[] _scenesToInject;
        [SerializeField] private UnityEvent _onStartInject;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetCache()
        {
            _loadedScenePaths.Clear();
        }

        // OnValidate won't be called when changing tag manually
        private void Update()
        {
            if (Application.isPlaying) { return; }
            if (!CompareTag(EditorOnlyTag))
            {
                Debug.LogWarning($"{nameof(SceneInjector)} GameObject is auto-tagged as {EditorOnlyTag}");
                gameObject.tag = EditorOnlyTag;
                var suffix = $" ({EditorOnlyTag})";
                if (gameObject.name.EndsWith(suffix)) { gameObject.name += suffix; }
            }
        }

        private void Awake()
        {
            if (!Application.isPlaying) { return; }

            Destroy(gameObject);
            InjectScene().Forget();
        }

        private async UniTaskVoid InjectScene()
        {
            var scene = gameObject.scene;
            var scenePath = scene.path;
            if (_loadedScenePaths.Contains(scenePath)) { return; }
            _loadedScenePaths.Add(scenePath);

            _onStartInject?.Invoke();

            foreach (var rootObject in gameObject.scene.GetRootGameObjects())
            {
                rootObject.SetActive(false);
            }

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.path == scenePath) { continue; }
                _loadedScenePaths.Add(loadedScene.path);
            }

            foreach (var injectingScene in _scenesToInject)
            {
                var injectingScenePath = AssetDatabase.GetAssetPath(injectingScene);
                if (_loadedScenePaths.Contains(injectingScenePath)) { continue; }

                await EditorSceneManager.LoadSceneAsyncInPlayMode(injectingScenePath, new LoadSceneParameters(LoadSceneMode.Additive));
            }

            await SceneManager.UnloadSceneAsync(scene);
            await EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Additive));

        }
    }
}

#endif