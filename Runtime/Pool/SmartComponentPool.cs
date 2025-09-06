using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Pool;

namespace GameDevKit.Pool
{
    [Serializable]
    public class ParticleSystemSmartPool : SmartComponentPool<ParticleSystem>
    {
        private bool _isInit;

        public ParticleSystemSmartPool(ParticleSystem template, float autoReleaseTime = -1) : base(template, autoReleaseTime) { }

        public override ParticleSystem GetAndAutoPool(Vector3? position = null, TimeSpan? customAutoReleaseTime = null)
        {
            if (_template == null) { return null; }
            if (!_isInit)
            {
                _isInit = true;
                var sceneTemplate = Pool.Template;
                var mainModule = sceneTemplate.main;
                if (mainModule.stopAction == ParticleSystemStopAction.Destroy)
                {
                    Debug.Log($"{GetPoolName(_template)} scene template has a stop action of Destroy. Changing it to None.");
                    mainModule.stopAction = ParticleSystemStopAction.None;
                }
                mainModule.playOnAwake = true;
            }
            var pooledObj = base.GetAndAutoPool(position, customAutoReleaseTime);
            return pooledObj;
        }
    }

    [Serializable]
    public class SmartComponentPool<T> where T : Component
    {
        [Required]
        [SerializeField] protected T _template;

        [Tooltip("Entities that use the same template (prefab or scene object) will share a pool. This reduces the amount of pooled objects.")]
        [SerializeField] private bool _shareTemplatePool = true;

        [Tooltip("Time <= 0 means infinite")]
        [SerializeField, Min(-1)] protected float _autoReleaseTime = -1;

        private static readonly Dictionary<T, ComponentPool<T>> _sharedPools = new();

        private ComponentPool<T> _poolCached;
        public ComponentPool<T> Pool
        {
            get
            {
                if (_template == null) { return null; }

                if (_shareTemplatePool && _sharedPools.TryGetValue(_template, out var sharedPool))
                {
                    if (PoolIsValid(sharedPool))
                    {
                        return sharedPool;
                    }

                    _sharedPools.RemoveWhere(pair => !PoolIsValid(pair.Value));
                }

                if (PoolIsValid(_poolCached)) { return _poolCached; }

                var container = GetContainer(_template);
                _poolCached = new ComponentPool<T>(_template, container);
                if (_shareTemplatePool)
                {
                    _sharedPools[_template] = _poolCached;
                }
                return _poolCached;
            }
        }

        private static bool PoolIsValid(ComponentPool<T> pool) => pool != null && pool.Container != null;

        private SmartComponentPool() { }
        public SmartComponentPool(T template, float autoReleaseTime = -1)
        {
            _template = template;
            _autoReleaseTime = autoReleaseTime;
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

            var releaseSeconds = (float)(customAutoReleaseTime?.TotalSeconds ?? _autoReleaseTime);
            if (releaseSeconds <= 0) { return pooledObj; }

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
                        ReleaseOrDestroy();
                        return;
                    }
                    releaseSeconds -= Time.deltaTime;
                }
                ReleaseOrDestroy();

                void ReleaseOrDestroy()
                {
                    if (pooledObj == null) { return; }
                    if (Pool == null)
                    {
                        Debug.LogWarning($"Pool is null for {pooledObj.name}, destroying instead of releasing!");
                        UnityEngine.Object.Destroy(pooledObj.gameObject);
                        return;
                    }
                    Pool.Release(pooledObj);
                }
            }
        }
    }

}