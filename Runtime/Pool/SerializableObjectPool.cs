using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit.Pool
{
    [Serializable]
    public class SerializableObjectPool<T> : IObjectPool<T> where T : Component
    {
        [SerializeField] private T _template;

        private ObjectPool<T> _poolCached;
        private ObjectPool<T> pool { get => _poolCached ??= new ObjectPool<T>(_template); }

        public IReadOnlyCollection<T> ActiveElements => pool.ActiveElements;
        public IReadOnlyCollection<T> InactiveElements => pool.InactiveElements;

        public Action<T> OnInstantiate { get => pool.OnInstantiate; set => pool.OnInstantiate = value; }
        public Action<T> OnGet { get => pool.OnInstantiate; set => pool.OnInstantiate = value; }
        public Action<T> OnStore { get => pool.OnInstantiate; set => pool.OnInstantiate = value; }

        public T Get(bool activate = true) => pool.Get(activate);
        public void Store(T element, bool resetTransform = true) => pool.Store(element, resetTransform);
        public void StoreAll(bool resetTransform = true) => pool.StoreAll(resetTransform);
        public void Clear(bool destroyElements = false) => pool.Clear(destroyElements);
        public void Prepare(int minCount) => pool.Prepare(minCount);
    }
}