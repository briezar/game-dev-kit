using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameDevKit.SceneManagement
{
    public readonly struct SceneActivationHandle
    {
        public readonly string SceneName;

        private readonly AsyncOperation _sceneOp;

        public SceneActivationHandle(string sceneName, AsyncOperation sceneOp)
        {
            SceneName = sceneName;
            _sceneOp = sceneOp;
        }

        public async UniTask<Scene> Activate()
        {
            _sceneOp.allowSceneActivation = true;
            await _sceneOp;

            var scene = SceneManager.GetSceneByName(SceneName);
            return scene;
        }
    }

    public static class SceneUtils
    {
        public static async UniTask<Scene> LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive, Action<float> progressCallback = null)
        {
            var sceneOp = SceneManager.LoadSceneAsync(sceneName, mode);
            while (!sceneOp.isDone)
            {
                progressCallback?.Invoke(sceneOp.progress);
                await UniTask.Yield();
            }

            var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            return scene;
        }


        /// <summary>
        /// Loads a scene without activating it.
        /// The returned AsyncOperation will complete when the scene is loaded and ready to be activated, but the scene will not be activated until allowSceneActivation is set to true.
        /// </summary>
        public static async UniTask<SceneActivationHandle> LoadSceneWithoutActivation(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive, Action<float> progressCallback = null)
        {
            var sceneOp = SceneManager.LoadSceneAsync(sceneName, mode);
            sceneOp.allowSceneActivation = false;
            while (sceneOp.progress < 0.9f)
            {
                progressCallback?.Invoke(sceneOp.progress);
                await UniTask.Yield();
            }
            return new SceneActivationHandle(sceneName, sceneOp);
        }

        public static async UniTask UnloadScene(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(scene);
            }
        }
    }
}