using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameDevKit.SceneManagement
{
    public struct ProgressInfo
    {
        public float TargetProgress;
        public float EstimatedDuration;
        public string Message;

        public ProgressInfo(float targetProgress, float estimatedDuration, string message)
        {
            TargetProgress = targetProgress;
            EstimatedDuration = estimatedDuration;
            Message = message;
        }
    }

    [DefaultExecutionOrder(-1000)]
    public abstract class SceneFlow : MonoBehaviour
    {
        [field: SerializeField] public bool SetActiveSceneOnStart { get; private set; } = true;

        protected static readonly List<SceneFlow> _activeSceneFlows = new();

        protected void Start()
        {
            if (SetActiveSceneOnStart) { SetActiveScene(); }
            OnStart();
        }

        protected virtual async UniTaskVoid OnStart() { }

        protected void OnEnable() => _activeSceneFlows.Add(this);
        protected void OnDisable() => _activeSceneFlows.Remove(this);

        public virtual UniTask PrepareScene(Action<ProgressInfo> progressCallback = null) => UniTask.CompletedTask;

        public void SetActiveScene() => SceneManager.SetActiveScene(gameObject.scene);

        public async UniTask UnloadSelf() => await SceneManager.UnloadSceneAsync(gameObject.scene);

        public static T GetSceneFlow<T>() where T : SceneFlow => _activeSceneFlows.Find(s => s is T) as T;

        public static SceneFlow GetSceneFlow(string sceneName) => _activeSceneFlows.Find(s => s.gameObject.scene.name == sceneName);
        public static SceneFlow GetSceneFlowOfObject(Component component) => GetSceneFlowOfObject(component.gameObject);
        public static SceneFlow GetSceneFlowOfObject(GameObject gObj) => _activeSceneFlows.Find(s => s.gameObject.scene == gObj.scene);

        public static SceneFlow GetActiveSceneFlow()
        {
            var activeScene = SceneManager.GetActiveScene();
            return _activeSceneFlows.Find(s => s.gameObject.scene == activeScene);
        }

        public static async UniTask<SceneFlow> LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive, Action<float> progressCallback = null)
        {
            var scene = await SceneUtils.LoadScene(sceneName, mode, progressCallback);
            return FindInScene(scene);
        }

        public static SceneFlow FindInScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                Debug.LogError($"Invalid scene: {scene}");
                return null;
            }

            foreach (var obj in scene.GetRootGameObjects())
            {
                if (obj.TryGetComponentInChildren(out SceneFlow sceneFlow))
                {
                    return sceneFlow;
                }
            }
            return null;
        }
    }
}