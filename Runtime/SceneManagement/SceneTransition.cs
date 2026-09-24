using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameDevKit.UI;
using UnityEngine.SceneManagement;

namespace GameDevKit.SceneManagement
{
    public struct SceneTransitionOptions
    {
        public Action OnTransitionInComplete;
        public Action OnTransitionOutStart;
        public Action OnComplete;
    }

    public interface ISceneLoadStrategy
    {
        UniTask Execute(SceneDefinitionSO scene, SceneTransitionOptions options = default);
    }

    [Serializable]
    public class SingleSceneLoadStrategy : ISceneLoadStrategy
    {
        public async UniTask Execute(SceneDefinitionSO scene, SceneTransitionOptions options = default)
        {
            SceneFlow.LoadScene(scene.Scene.Path, LoadSceneMode.Single);
            await scene.LoadRequiredScenes();
        }
    }

    [Serializable]
    public class AdditiveSceneLoadStrategy : ISceneLoadStrategy
    {
        public async UniTask Execute(SceneDefinitionSO scene, SceneTransitionOptions options = default)
        {
            options.OnTransitionInComplete?.Invoke();

            var unloadTasks = scene.ScenesToUnload.Where(s => s.LoadedScene.IsValid()).Select(s => SceneManager.UnloadSceneAsync(s.Path).ToUniTask()).ToArray();
            var loadTask = SceneUtils.LoadSceneWithoutActivation(scene.Scene.Path);

            await UniTask.WhenAll(unloadTasks);
            await scene.LoadRequiredScenes();

            var sceneHandle = await loadTask;
            var newScene = await sceneHandle.Activate();

            if (newScene.IsValid())
            {
                var sceneFlow = SceneFlow.FindInScene(newScene);
                if (sceneFlow != null) { await sceneFlow.PrepareScene(); }
            }

            options.OnTransitionOutStart?.Invoke();
            options.OnComplete?.Invoke();
        }
    }

    [Serializable]
    public class UIManagerFadeLoadStrategy : ISceneLoadStrategy
    {
        public float FadeInDuration = 0.5f;
        public float FadeOutDuration = 0.5f;

        public async UniTask Execute(SceneDefinitionSO scene, SceneTransitionOptions options = default)
        {
            await UIManager.FadeTransition(FadeSetting.FadeIn(FadeInDuration));

            options.OnTransitionInComplete?.Invoke();

            var unloadTasks = scene.ScenesToUnload.Where(s => s.LoadedScene.IsValid()).Select(s => SceneManager.UnloadSceneAsync(s.Path).ToUniTask()).ToArray();
            var loadWithoutActivationTask = SceneUtils.LoadSceneWithoutActivation(scene.Scene.Path);

            await UniTask.WhenAll(unloadTasks);
            await scene.LoadRequiredScenes();

            var sceneHandle = await loadWithoutActivationTask;
            var newScene = await sceneHandle.Activate();

            if (newScene.IsValid())
            {
                var sceneFlow = SceneFlow.FindInScene(newScene);
                await sceneFlow.PrepareScene();
            }

            options.OnTransitionOutStart?.Invoke();
            await UIManager.FadeTransition(FadeSetting.FadeOut(FadeOutDuration));

            options.OnComplete?.Invoke();
        }
    }

}