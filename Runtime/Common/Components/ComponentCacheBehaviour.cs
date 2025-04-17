using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    public interface IComponentCache
    {
        void Cache(Component component);
        T Get<T>(int index = 0);
        void ClearCache();
    }

    [Serializable]
    public class ComponentCache : IComponentCache
    {
        [Tooltip("Be careful of assigning the wrong component!")]
        [SerializeField] protected List<Component> _components = new();

        public void Cache(Component component)
        {
            if (_components.Contains(component)) { return; }
            _components.Add(component);
        }

        public T Get<T>(int index = 0)
        {
            var componentIndex = 0;
            foreach (var component in _components)
            {
                if (component is not T targetComponent) { continue; }

                if (componentIndex != index)
                {
                    componentIndex++;
                    continue;
                }
                return targetComponent;

            }

            Debug.LogWarning($"Cannot find component {typeof(T).Name}!");
            return default;
        }

        public void ClearCache()
        {
            _components.Clear();
        }
    }

    public class ComponentCacheBehaviour : MonoBehaviour, IComponentCache
    {
        [Tooltip("Toggle this to debug which component was cached")]
        [SerializeField] private bool _debug;

        [Tooltip("Be careful of assigning the wrong component!")]
        [SerializeField] private Component[] _cachedComponents;

        private ComponentCache _cacher;

        private ComponentCache cacher
        {
            get
            {
                if (_cacher == null)
                {
                    _cacher = new();
                    foreach (var component in _cachedComponents)
                    {
                        _cacher.Cache(component);
                        if (_debug)
                        {
                            Debug.Log($"Cached {component}");
                        }
                    }
                    _cachedComponents = null;

                }
                return _cacher;
            }
        }

        public void Cache(Component component) => cacher.Cache(component);
        public T Get<T>(int index = 0) => cacher.Get<T>(index);
        public void ClearCache() => cacher.ClearCache();
    }
}
