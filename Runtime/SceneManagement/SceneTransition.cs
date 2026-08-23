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

    public interface ISceneTransition
    {
        UniTask Execute(SceneDefinitionSO scene, SceneTransitionOptions options = default);
    }

    [Serializable]
    public class SingleSceneTransition : ISceneTransition
    {
        public async UniTask Execute(SceneDefinitionSO scene, SceneTransitionOptions options = default)
        {
            SceneFlow.LoadScene(scene.Scene.Path, LoadSceneMode.Single);
        }
    }

    [Serializable]
    public class UIManagerFadeTransition : ISceneTransition
    {
        public async UniTask Execute(SceneDefinitionSO scene, SceneTransitionOptions options = default)
        {
            await UIManager.FadeTransition(FadeSetting.FadeIn());

            options.OnTransitionInComplete?.Invoke();

            var unloadTasks = scene.ScenesToUnload.Where(s => s.LoadedScene.IsValid()).Select(s => SceneManager.UnloadSceneAsync(s.Path).ToUniTask()).ToArray();
            var loadTask = SceneFlow.LoadSceneWithoutActivation(scene.Scene.Path);

            await UniTask.WhenAll(unloadTasks);

            var sceneHandle = await loadTask;
            var sceneFlow = await sceneHandle.Activate();

            if (sceneFlow != null)
            {
                await sceneFlow.PrepareScene();
            }

            options.OnTransitionOutStart?.Invoke();
            await UIManager.FadeTransition(FadeSetting.FadeOut());

            options.OnComplete?.Invoke();
        }
    }

}