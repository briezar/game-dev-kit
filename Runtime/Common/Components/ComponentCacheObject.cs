using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    public interface IComponentCache
    {
        void Cache(Component component);
        T Get<T>(int index = 0) where T : Component;
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

        public T Get<T>(int index = 0) where T : Component
        {
            var componentIndex = 0;
            (int index, T component) fallback = (0, null);
            foreach (var component in _components)
            {
                if (component is T targetComponent)
                {
                    fallback = (componentIndex, targetComponent);
                    if (componentIndex != index)
                    {
                        componentIndex++;
                        continue;
                    }
                    return targetComponent;
                }
            }

            if (fallback.component != null)
            {
                Debug.LogWarning($"Cannot find component {typeof(T).Name} at index {index}. Fallback to index {fallback.index}!", fallback.component);
                return fallback.component;
            }

            Debug.LogWarning($"Cannot find component {typeof(T).Name}!");
            return default;
        }

        public void ClearCache()
        {
            _components.Clear();
        }
    }

    public class ComponentCacheObject : MonoBehaviour, IComponentCache
    {
        [Tooltip("Toggle this to debug which component was cached")]
        [SerializeField] private bool _debug;

        [Tooltip("Be careful of assigning the wrong component!")]
        [SerializeField] private Component[] _cachedComponents;

        private ComponentCache _internalCache;
        private ComponentCache _cache
        {
            get
            {
                if (_internalCache == null)
                {
                    _internalCache = new();
                    foreach (var component in _cachedComponents)
                    {
                        _internalCache.Cache(component);
                        if (_debug)
                        {
                            Debug.Log($"Cached {component}", component);
                        }
                    }
                    _cachedComponents = null;

                }
                return _internalCache;
            }
        }

        public void Cache(Component component) => _cache.Cache(component);
        public T Get<T>(int index = 0) where T : Component => _cache.Get<T>(index);
        public void ClearCache() => _cache.ClearCache();
    }
}
