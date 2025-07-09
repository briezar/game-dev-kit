using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit.Pool
{
    [Serializable]
    public class SerializableObjectPool<T> : IObjectPool<T> where T : Component
    {
        [SerializeField] private T _template;

        [Tooltip("If not set, will use the template's parent")]
        [SerializeField] private Transform _parent;

        [Tooltip("If true and template is a prefab, will instantiate a scene template from the prefab, then pool from the scene template. If false, will pool from the prefab. This is useful for altering the template in the scene without affecting the prefab.")]
        [SerializeField] private bool _instantiateSceneTemplate = true;

        private ObjectPool<T> _poolCached;
        private ObjectPool<T> pool { get => _poolCached ??= new ObjectPool<T>(_template, _parent ? _parent : _template.transform.parent) { InstantiateSceneTemplate = _instantiateSceneTemplate }; }

        public IReadOnlyCollection<T> ActiveElements => pool.ActiveElements;
        public IReadOnlyCollection<T> InactiveElements => pool.InactiveElements;

        public Action<T> OnInstantiate { get => pool.OnInstantiate; set => pool.OnInstantiate = value; }
        public Action<T> OnGet { get => pool.OnGet; set => pool.OnGet = value; }
        public Action<T> OnStore { get => pool.OnStore; set => pool.OnStore = value; }

        public T Get(bool activate = true) => pool.Get(activate);
        public void Store(T element, bool resetTransform = true) => pool.Store(element, resetTransform);
        public void StoreAll(bool resetTransform = true) => pool.StoreAll(resetTransform);
        public void Clear() => pool.Clear();
        public void Prepare(int minCount) => pool.Prepare(minCount);
    }
}