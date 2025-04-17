using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameDevKit.Pool
{
    public interface IPoolableObjectEventReceiver
    {
        void OnStore();
        void OnGet();
    }

    public interface IObjectPool<T> where T : Component
    {
        IReadOnlyCollection<T> ActiveElements { get; }
        IReadOnlyCollection<T> InactiveElements { get; }
        Action<T> OnInstantiate { get; set; }
        Action<T> OnGet { get; set; }
        Action<T> OnStore { get; set; }

        void Prepare(int minCount);
        T Get(bool activate = true);
        void Store(T element, bool resetTransform = true);
        void StoreAll(bool resetTransform = true);
        void Clear(bool destroyElements = false);
    }

    public static class ObjectPoolExtensions
    {
        public static T Get<T>(this IObjectPool<T> objectPool, Vector3 position, bool activate = true) where T : Component
        {
            var element = objectPool.Get(false);
            element.transform.position = position;
            element.gameObject.SetActive(activate);
            return element;
        }

        public static T Get<T>(this IObjectPool<T> objectPool, Transform parent, bool activate = true) where T : Component
        {
            var element = objectPool.Get(false);
            element.transform.SetParent(parent, false);
            element.transform.localPosition = Vector3.zero;
            element.gameObject.SetActive(activate);
            return element;
        }
    }

    public class ObjectPool<T> : IObjectPool<T> where T : Component
    {
        private readonly T _template;
        private readonly HashSet<T> _activeSet = new();
        private readonly Stack<T> _inactiveStack = new();
        private readonly Transform _parent;

        public Action<T> OnInstantiate { get; set; }
        public Action<T> OnGet { get; set; }
        public Action<T> OnStore { get; set; }

        public IReadOnlyCollection<T> ActiveElements => _activeSet;
        public IReadOnlyCollection<T> InactiveElements => _inactiveStack;

        /// <summary> Be careful! Editing this will affect all subsequent Get() calls. Use this to reset to default values </summary>
        public T OriginalTemplate => _template;
        private T _sceneTemplate;

        /// <summary> Be careful! Editing this will affect all subsequent Get() calls. Use this to reset to default values </summary>
        public T GetSceneTemplate()
        {
            if (_sceneTemplate == null)
            {
                _sceneTemplate = _template.IsPrefab() ? Object.Instantiate(_template, _parent) : _template;
                _sceneTemplate.gameObject.SetActive(false);
            }
            return _sceneTemplate;
        }

        public ObjectPool(T element) : this(element, element.transform.parent) { }

        public ObjectPool(T template, Transform parent)
        {
            _template = template;
            _parent = parent;
            GetSceneTemplate();
        }

        public void Prepare(int minCount)
        {
            var currentCount = _inactiveStack.Count + _activeSet.Count;
            for (int i = 0; i < minCount - currentCount; i++)
            {
                var element = Object.Instantiate(GetSceneTemplate(), _parent);
                element.gameObject.SetActive(false);
                _inactiveStack.Push(element);
            }
        }

        public T Get(bool activate = true)
        {
            T element;

            while (_inactiveStack.TryPop(out element) && element == null) { }

            if (element == null)
            {
                element = Object.Instantiate(GetSceneTemplate(), _parent);
                OnInstantiate?.Invoke(element);
            }

            if (activate)
            {
                element.gameObject.SetActive(true);
            }

            _activeSet.Add(element);

            OnGet?.Invoke(element);
            if (element is IPoolableObjectEventReceiver receiver)
            {
                receiver.OnGet();
            }
            return element;

        }

        public void Store(T element, bool resetTransform = true)
        {
            if (element == GetSceneTemplate()) { return; }
            if (element == null)
            {
                _activeSet.RemoveWhere(e => e == null);
                return;
            }

            if (!_activeSet.Remove(element)) { return; }

            InternalStore(element, resetTransform);
        }

        private void InternalStore(T element, bool resetTransform = true)
        {
            element.gameObject.SetActive(false);
            element.transform.SetParent(_parent);

            if (resetTransform)
            {
                element.transform.rotation = GetSceneTemplate().transform.rotation;
                element.transform.localScale = GetSceneTemplate().transform.localScale;
            }

            OnStore?.Invoke(element);
            if (element is IPoolableObjectEventReceiver receiver)
            {
                receiver.OnStore();
            }
            _inactiveStack.Push(element);
        }

        public void StoreAll(bool resetTransform = true)
        {
            foreach (var element in _activeSet)
            {
                InternalStore(element, resetTransform);
            }

            _activeSet.Clear();
        }

        public void Clear(bool destroyElements = false)
        {
            if (destroyElements)
            {
                foreach (var element in _inactiveStack)
                {
                    Object.Destroy(element.gameObject);
                }
                foreach (var element in _activeSet)
                {
                    Object.Destroy(element.gameObject);
                }
            }

            _inactiveStack.Clear();
            _activeSet.Clear();
        }
    }
}