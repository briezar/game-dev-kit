using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Pool;

namespace GameDevKit.Pool
{
    /// <summary>
    /// Simplifies the usage of ComponentPool with <see cref="GetAndAutoPool"/>.<br/>
    /// Allows sharing pools between entities that use the same template (prefab or scene object).
    /// </summary>
    [Serializable]
    public class SmartComponentPool<T> where T : Component
    {
        [Required]
        [SerializeField] protected T _template;

        [field: Tooltip("Entities that use the same template (prefab or scene object) will share a pool. This reduces the amount of pooled objects.")]
        [field: SerializeField] public bool ShareTemplatePool { get; set; } = true;

        [field: Tooltip("Time < 0 means infinite")]
        [field: SerializeField, Min(-1)] public float AutoReleaseTime { get; set; } = -1;

        private static readonly Dictionary<T, ComponentPool<T>> _sharedPools = new();

        private ComponentPool<T> _poolCached;
        public ComponentPool<T> Pool
        {
            get
            {
                if (!Application.isPlaying)
                {
                    Debug.LogWarning("Pools are only available in Play Mode!");
                    return null;
                }

                if (_template == null) { return null; }

                if (ShareTemplatePool && _sharedPools.TryGetValue(_template, out var sharedPool))
                {
                    if (sharedPool.IsValid())
                    {
                        return sharedPool;
                    }

                    _sharedPools.RemoveWhere(pair => !pair.Value.IsValid());
                }

                if (_poolCached.IsValid()) { return _poolCached; }

                var container = GetContainer(_template);
                _poolCached = new ComponentPool<T>(_template, container);
                if (ShareTemplatePool)
                {
                    _sharedPools[_template] = _poolCached;
                }
                return _poolCached;
            }
        }


        private SmartComponentPool() { }
        public SmartComponentPool(T template, float autoReleaseTime = -1)
        {
            _template = template;
            AutoReleaseTime = autoReleaseTime;
        }

        protected static string GetPoolName(T template) => $"{template.name} ({template.GetType().Name}) Pool";

        private static Transform GetContainer(T template)
        {
            using var _ = ListPool<GameObject>.Get(out var rootObjects);
            template.gameObject.scene.GetRootGameObjects(rootObjects);
            var poolName = GetPoolName(template);
            var containerObj = rootObjects.Find(obj => obj.name.FastOrdinalEquals(poolName));
            if (containerObj == null)
            {
                containerObj = new GameObject(poolName);
                if (containerObj.scene != template.gameObject.scene)
                {
                    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(containerObj, template.gameObject.scene);
                }
            }
            return containerObj.transform;
        }

        public void Prepare(int minCount = 1) => Pool.Prepare(minCount);

        public virtual T GetAndAutoPool(Vector3? position = null, TimeSpan? customAutoReleaseTime = null)
        {
            var pooledObj = Pool.Get(false);
            if (position != null) { pooledObj.transform.position = position.Value; }
            pooledObj.gameObject.SetActive(true);

            var releaseSeconds = (float)(customAutoReleaseTime?.TotalSeconds ?? AutoReleaseTime);
            if (releaseSeconds < 0) { return pooledObj; }

            ScheduleReturnToPool();
            return pooledObj;

            async UniTaskVoid ScheduleReturnToPool()
            {
                while (releaseSeconds > 0)
                {
                    await UniTask.Yield();
                    if (pooledObj == null) { return; }
                    if (!pooledObj.gameObject.activeSelf)
                    {
                        this.ReleaseOrDestroy(pooledObj);
                        return;
                    }
                    releaseSeconds -= Time.deltaTime;
                }
                this.ReleaseOrDestroy(pooledObj);
            }
        }
    }

    public static class SmartComponentPoolExtensions
    {
        public static bool IsValid<T>(this SmartComponentPool<T> smartPool) where T : Component => smartPool?.Pool?.Container != null;

        public static void ReleaseOrDestroy<T>(this SmartComponentPool<T> smartPool, T pooledObj) where T : Component
        {
            if (pooledObj == null) { return; }
            if (!smartPool.IsValid())
            {
                Debug.LogWarning($"Pool is null for {pooledObj.name}, destroying instead of releasing!");
                UnityEngine.Object.Destroy(pooledObj.gameObject);
                return;
            }
            smartPool.Pool.Release(pooledObj);
        }
    }

}