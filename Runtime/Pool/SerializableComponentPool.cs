using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit.Pool
{
    [Serializable]
    public class SerializableComponentPool<T> : IComponentPool<T> where T : Component
    {
        [SerializeField] private T _template;
        [SerializeField] private Transform _parent;

        [Tooltip("If true and template is a prefab, will instantiate a scene template from the prefab, then pool from the scene template. If false, will pool from the prefab. This is useful for altering the template in the scene without affecting the prefab.")]
        [SerializeField] private bool _instantiateSceneTemplate = true;

        private ComponentPool<T> _poolCached;
        private ComponentPool<T> pool { get => _poolCached ??= new ComponentPool<T>(_template, _parent, _instantiateSceneTemplate); }

        public IReadOnlyCollection<T> ActiveElements => pool.ActiveElements;
        public IReadOnlyCollection<T> InactiveElements => pool.InactiveElements;

        public Action<T> OnInstantiate { get => pool.OnInstantiate; set => pool.OnInstantiate = value; }
        public Action<T> OnGet { get => pool.OnGet; set => pool.OnGet = value; }
        public Action<T> OnRelease { get => pool.OnRelease; set => pool.OnRelease = value; }

        public T Get(bool activate = true) => pool.Get(activate);
        public void Release(T element, bool resetTransform = true) => pool.Release(element, resetTransform);
        public void ReleaseAll(bool resetTransform = true) => pool.ReleaseAll(resetTransform);
        public void Clear() => pool.Clear();
        public void Prepare(int minCount) => pool.Prepare(minCount);
    }
}