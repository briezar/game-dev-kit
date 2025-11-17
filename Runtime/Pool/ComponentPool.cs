using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameDevKit.Pool
{
    public interface IObjectPoolEventReceiver
    {
        void HandleOnRelease();
        void HandleOnGet();
    }

    public interface IComponentPool<T> where T : Component
    {
        IReadOnlyCollection<T> ActiveElements { get; }
        IReadOnlyCollection<T> InactiveElements { get; }
        Action<T> OnInstantiate { get; set; }
        Action<T> OnGet { get; set; }
        Action<T> OnRelease { get; set; }

        void Prepare(int minCount);
        T Get(bool activate = true);
        void Release(T element, bool resetTransform = true);
        void ReleaseAll(bool resetTransform = true);
        void Clear();
    }

    public class ComponentPool<T> : IComponentPool<T> where T : Component
    {
        public Action<T> OnInstantiate { get; set; }
        public Action<T> OnGet { get; set; }
        public Action<T> OnRelease { get; set; }

        public IReadOnlyCollection<T> ActiveElements => _activeSet;
        public IReadOnlyCollection<T> InactiveElements => _inactiveStack;

        public readonly bool InstantiateSceneTemplate;

        /// <summary> Editing this can affect prefab data if template is a prefab!  </summary>
        public T OriginalTemplate => _template;

        /// <summary> 
        /// Editing this will affect all subsequent Get() calls. Be careful! Use this to set default values. <br/>
        /// This can also affect prefab data if template is a prefab and <see cref="InstantiateSceneTemplate"/> is false.
        /// </summary>
        public T Template
        {
            get
            {
                if (!InstantiateSceneTemplate) { return _template; }
                if (_sceneTemplate != null) { return _sceneTemplate; }

                _sceneTemplate = _template.IsPrefab() ? Object.Instantiate(_template, Container) : _template;
                _sceneTemplate.gameObject.SetActive(false);
                return _sceneTemplate;
            }
        }

        public readonly Transform Container;

        private readonly T _template;
        private readonly HashSet<T> _activeSet = new();
        private readonly Stack<T> _inactiveStack = new();

        private T _sceneTemplate;

        public ComponentPool(T template, Transform container, bool instantiateSceneTemplate = true)
        {
            _template = template;
            Container = container;
            InstantiateSceneTemplate = instantiateSceneTemplate;

            if (instantiateSceneTemplate)
            {
                Template.gameObject.SetActive(false);
            }
        }

        private readonly Dictionary<T, Action<T>> _pendingUpdates = new();

        /// <summary>
        /// Updates the template.<br/>
        /// If <paramref name="updateActive"/> is true, also updates all active elements, otherwise queues the update to be applied on release.<br/>
        /// Inactive elements are always updated.
        /// </summary>
        public void UpdateTemplate(Action<T> updateAction, bool updateActive = false)
        {
            updateAction?.Invoke(Template);

            foreach (var element in _inactiveStack)
            {
                updateAction?.Invoke(element);
            }

            foreach (var element in _activeSet)
            {
                if (updateActive)
                {
                    updateAction?.Invoke(element);
                }
                else
                {
                    _pendingUpdates.TryAdd(element, null);
                    _pendingUpdates[element] += updateAction;
                }
            }
        }

        public void Prepare(int minCount)
        {
            var currentCount = _inactiveStack.Count + _activeSet.Count;
            for (int i = 0; i < minCount - currentCount; i++)
            {
                var element = InstantiateElement();
                element.gameObject.SetActive(false);
                _inactiveStack.Push(element);
            }
        }

        private T InstantiateElement()
        {
            var element = Object.Instantiate(Template, Container);
            OnInstantiate?.Invoke(element);
            return element;
        }

        public T Get(bool activate = true)
        {
            T element;

            while (_inactiveStack.TryPop(out element) && element == null) { }

            if (element == null)
            {
                element = InstantiateElement();
            }

            if (activate)
            {
                element.gameObject.SetActive(true);
            }

            _activeSet.Add(element);

            OnGet?.Invoke(element);
            if (element is IObjectPoolEventReceiver receiver)
            {
                receiver.HandleOnGet();
            }
            return element;

        }

        public void Release(T element, bool resetTransform = true)
        {
            if (element == Template) { return; }
            if (element == null)
            {
                _activeSet.RemoveWhere(match => match == null);
                return;
            }

            if (!_activeSet.Remove(element)) { return; }

            InternalRelease(element, resetTransform);
        }

        private void InternalRelease(T element, bool resetTransform = true)
        {
            element.gameObject.SetActive(false);
            element.transform.SetParent(Container);

            if (resetTransform)
            {
                element.transform.rotation = Quaternion.identity;
                element.transform.localScale = Template.transform.localScale;
            }

            OnRelease?.Invoke(element);
            if (element is IObjectPoolEventReceiver receiver)
            {
                receiver.HandleOnRelease();
            }

            if (_pendingUpdates.Remove(element, out var updateAction))
            {
                updateAction?.Invoke(element);
            }

            _inactiveStack.Push(element);
        }

        public void ReleaseAll(bool resetTransform = true)
        {
            foreach (var element in _activeSet)
            {
                InternalRelease(element, resetTransform);
            }

            _activeSet.Clear();
        }

        public void Clear()
        {
            ClearActive();
            ClearInactive();
        }

        public void ClearActive()
        {
            foreach (var element in _activeSet)
            {
                Object.Destroy(element.gameObject);
            }
            _activeSet.Clear();
        }

        public void ClearInactive()
        {
            foreach (var element in _inactiveStack)
            {
                Object.Destroy(element.gameObject);
            }
            _inactiveStack.Clear();
        }
    }

    public static class ComponentPoolExtensions
    {
        public static bool IsValid<T>(this ComponentPool<T> pool) where T : Component => pool?.Container != null;

        public static T Get<T>(this IComponentPool<T> objectPool, Vector3 position, bool activate = true) where T : Component
        {
            var element = objectPool.Get(false);
            element.transform.position = position;
            element.gameObject.SetActive(activate);
            return element;
        }

        public static T Get<T>(this IComponentPool<T> objectPool, Transform parent, bool activate = true) where T : Component
        {
            var element = objectPool.Get(false);
            element.transform.SetParent(parent, false);
            element.transform.localPosition = Vector3.zero;
            element.gameObject.SetActive(activate);
            return element;
        }
    }
}