using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using Eflatun.SceneReference;
using GameDevKit.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace GameDevKit.SceneManagement
{
    [CreateAssetMenu(menuName = "GameDevKit/SceneManagement/SceneDefinition")]
    public class SceneDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public SceneReference Scene { get; private set; }
        [field: SerializeField] public SceneReference[] ScenesToUnload { get; private set; }
        [field: SerializeField] public SceneReference[] RequiredScenes { get; private set; }

        [SubclassPicker]
        [SerializeReference] private ISceneLoadStrategy _loadStrategy = new AdditiveSceneLoadStrategy();

        public UniTask LoadScene(SceneTransitionOptions options = default) => LoadScene(_loadStrategy, options);
        public UniTask LoadScene(ISceneLoadStrategy loadStrategy, SceneTransitionOptions options = default) => loadStrategy.Execute(this, options);

        public UniTask LoadRequiredScenes() => UniTask.WhenAll(RequiredScenes.Select(EnsureLoadedAsync));

        public async UniTask UnloadScene()
        {
            if (!Scene.ExistsInBuild())
            {
                Debug.LogWarning($"Scene '{Scene.Path}' does not exist in the build settings.");
                return;
            }
            if (!Scene.LoadedScene.IsValid() || !Scene.LoadedScene.isLoaded) // is not loaded
            {
                return;
            }

            await SceneManager.UnloadSceneAsync(Scene.LoadedScene);
        }

        private static async UniTask EnsureLoadedAsync(SceneReference sceneRef)
        {
            if (!sceneRef.ExistsInBuild())
            {
                Debug.LogWarning($"Scene '{sceneRef.Path}' does not exist in the build settings.");
                return;
            }
            if (!sceneRef.LoadedScene.IsValid()) // is not loaded
            {
                await sceneRef.LoadSceneAsync(LoadSceneMode.Additive);
                return;
            }
            if (sceneRef.LoadedScene.isLoaded) { return; }

            // is loading
            var success = await UniTask.WaitUntil(() => sceneRef.LoadedScene.IsValid()).TimeoutWithoutException(TimeSpan.FromSeconds(10));
            if (!success)
            {
                Debug.LogError($"Scene '{sceneRef.Path}' failed to load within the timeout period.");
            }
        }

#if UNITY_EDITOR
        [Button("Open Scenes", serializeParameters: false)]
        private void Editor_SetupScenes(bool addToOpenedScenes = true)
        {
            if (!EditorUtility.DisplayDialog("Open Scenes", "This will open the main scene and all required scenes. Continue?", "Yes", "No"))
            {
                return;
            }

            EditorSceneManager.OpenScene(Scene.Path, addToOpenedScenes ? OpenSceneMode.Additive : OpenSceneMode.Single);
            foreach (var scene in RequiredScenes)
            {
                EditorSceneManager.OpenScene(scene.Path, OpenSceneMode.Additive);
            }
        }

#endif
    }

}
