using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameDevKit.SceneManagement
{
    public static class SceneReferenceExtensions
    {
        public static string GetValidScenePath(this SceneReference sceneRef) => sceneRef.Path.RemoveFirst("Assets/").RemoveLast(".unity");
        public static async UniTask LoadSceneAsync(this SceneReference sceneRef, LoadSceneMode mode) => await SceneManager.LoadSceneAsync(GetValidScenePath(sceneRef), mode);
        public static void LoadScene(this SceneReference sceneRef, LoadSceneMode mode) => SceneManager.LoadScene(GetValidScenePath(sceneRef), mode);
        public static async UniTask UnloadScene(this SceneReference sceneRef)
        {
            var scene = SceneManager.GetSceneByPath(GetValidScenePath(sceneRef));
            if (scene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(scene);
            }
        }
    }
}