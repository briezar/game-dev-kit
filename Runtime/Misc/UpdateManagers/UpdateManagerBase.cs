using System;
using System.Collections.Generic;
using GameDevKit.Collections;
using UnityEngine;

namespace GameDevKit.UpdateManagers
{
    /// <summary>
    /// Manages the registration and updating of updatable objects in a type-coherent manner for optimal performance.
    /// </summary>
    public abstract class UpdateManagerBase<TManager, TUpdatable> : SingletonBehaviour<TManager> where TManager : UpdateManagerBase<TManager, TUpdatable>
    {
        // Grouping by Type gives massive Instruction Cache (I-Cache) locality and highly predictable branching
        private readonly Dictionary<Type, int> _typeToBucketId = new();
        private int _nextBucketId = 0;

        private readonly List<SwapBackList<TUpdatable>> _buckets = new();
        private int _updatableCount = 0;
        private bool _isUpdating = false;

        private readonly Dictionary<TUpdatable, bool> _pendingActions = new();

        public static void Register(TUpdatable updatable) => GetInstance()?.InternalRegister(updatable);
        public static void Unregister(TUpdatable updatable) => GetInstance()?.InternalUnregister(updatable);
        public static void Clear() => GetInstance()?.InternalClear();

        protected override void OnValidAwake() => DontDestroyOnLoad(gameObject);

        private static TManager GetInstance()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) { throw new InvalidOperationException("UpdateManager can only be accessed during play mode."); }
#endif

            if (_instance == null)
            {
                var go = new GameObject(typeof(TManager).Name);
                _instance = go.AddComponent<TManager>();
            }
            return _instance;
        }

        private void InternalRegister(TUpdatable updatable)
        {
            if (updatable == null) { return; }
            if (_isUpdating)
            {
                _pendingActions[updatable] = true;
                return;
            }

            var type = updatable.GetType();

            if (!_typeToBucketId.TryGetValue(type, out int bucketId))
            {
                bucketId = _nextBucketId++;
                _typeToBucketId.Add(type, bucketId);
                _buckets.Add(new());
            }

            if (_buckets[bucketId].Add(updatable))
            {
                _updatableCount++;
                enabled = true;
            }
        }

        private void InternalUnregister(TUpdatable updatable)
        {
            if (updatable == null) { return; }
            if (_isUpdating)
            {
                _pendingActions[updatable] = false;
                return;
            }

            var type = updatable.GetType();
            if (_typeToBucketId.TryGetValue(type, out int bucketId))
            {
                if (_buckets[bucketId].Remove(updatable))
                {
                    _updatableCount--;
                }
            }

            enabled = _updatableCount > 0;
        }

        private void InternalClear()
        {
            _typeToBucketId.Clear();
            foreach (var bucket in _buckets)
            {
                bucket.Clear(); // Clear each bucket individually to stop Poll() from iterating over the current bucket
            }
            _buckets.Clear();
            _nextBucketId = 0;
            _updatableCount = 0;
            enabled = false;

            _pendingActions.Clear();
        }

        protected void Poll(float deltaTime)
        {
            _isUpdating = true;
            for (int b = 0; b < _buckets.Count; b++)
            {
                var bucket = _buckets[b];
                for (int i = 0; i < bucket.Count; i++)
                {
                    var updatable = bucket[i];
                    if (updatable is UnityEngine.Object unityObject && unityObject == null)
                    {
                        InternalUnregister(updatable);
                        continue;
                    }

                    try
                    {
                        UpdateUpdatable(updatable, deltaTime);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error while updating updatable: {updatable} ({updatable.GetType()})\n{ex}", updatable as UnityEngine.Object);
                    }
                }
            }
            _isUpdating = false;

            FlushPendingModifications();
        }

        private void FlushPendingModifications()
        {
            if (_pendingActions.Count == 0) { return; }

            foreach (var (updatable, isRegister) in _pendingActions)
            {
                if (isRegister) { InternalRegister(updatable); }
                else { InternalUnregister(updatable); }
            }
            _pendingActions.Clear();
        }

        protected abstract void UpdateUpdatable(TUpdatable updatable, float deltaTime);
    }

}