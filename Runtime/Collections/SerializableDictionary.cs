using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameDevKit.Collections
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IDictionary<TKey, TValue>
    {
        [SerializeField] private KeyValuePair[] _keyValuePairs;

        public int Count => _keyValuePairs.Length;

        public IEnumerable<TKey> Keys
        {
            get
            {
                if (_keyValuePairs == null) { yield break; }
                foreach (var pair in _keyValuePairs)
                {
                    yield return pair.Key;
                }
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                if (_keyValuePairs == null) { yield break; }
                foreach (var pair in _keyValuePairs)
                {
                    yield return pair.Value;
                }
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys.ToArray();
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values.ToArray();
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;
        TValue IDictionary<TKey, TValue>.this[TKey key] { get => GetValue(key); set => throw new InvalidOperationException(); }

        public TValue this[TKey key] => GetValue(key);

        //     void ISerializationCallbackReceiver.OnBeforeSerialize()
        //     {
        // #if UNITY_EDITOR
        //         if (_keyValuePairs.IsNullOrEmpty()) { return; }

        //         var duplicateQuery = Keys.GroupBy(key => key)
        //                                  .Where(keyGroup => keyGroup.Count() > 1)
        //                                  .Select(keyGroup => new { keyGroup.Key, Count = keyGroup.Count() });

        //         foreach (var duplicate in duplicateQuery)
        //         {
        //             Debug.LogError($"Found {duplicate.Count} duplicate keys for: {duplicate.Key}");
        //         }
        // #endif
        //     }

        // void ISerializationCallbackReceiver.OnAfterDeserialize() { }

#if UNITY_EDITOR
        public void CheckDuplicateKeys(out bool hasDuplicateKeys)
        {
            hasDuplicateKeys = false;
            if (_keyValuePairs.IsNullOrEmpty()) { return; }

            var duplicateQuery = Keys.GroupBy(key => key)
                                     .Where(keyGroup => keyGroup.Count() > 1)
                                     .Select(keyGroup => new { keyGroup.Key, Count = keyGroup.Count() });

            if (duplicateQuery.IsNullOrEmpty()) { return; }
            hasDuplicateKeys = true;

            foreach (var duplicate in duplicateQuery)
            {
                Debug.LogError($"Found {duplicate.Count} duplicate keys for: {duplicate.Key}");
            }

            return;
        }
#endif

        private TValue GetValue(TKey key)
        {
            if (TryGetValue(key, out var value))
            {
                return value;
            }

            Debug.LogError($"Key not found: {key}, returned {default} (SerializableDictionary<{typeof(TKey).Name},{typeof(TValue).Name}>)");
            return default;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            foreach (var entry in _keyValuePairs)
            {
                if (entry.Key.Equals(key))
                {
                    value = entry.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public TValue Find(Func<KeyValuePair, bool> match)
        {
            if (match == null) { return default; }

            foreach (var pair in _keyValuePairs)
            {
                if (match(pair)) { return pair.Value; }
            }
            return default;
        }

        public bool ContainsKey(TKey key)
        {
            /// Arrays are faster than dictionary with less than 15 elements, consider implementing Dictionary for lookup if the collection is large
            return _keyValuePairs.Exists(entry => entry.Key.Equals(key));
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (_keyValuePairs == null) { yield break; }
            foreach (var entry in _keyValuePairs)
            {
                yield return new(entry.Key, entry.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => throw new InvalidOperationException();
        bool IDictionary<TKey, TValue>.Remove(TKey key) => throw new InvalidOperationException();
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => throw new InvalidOperationException();
        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => throw new InvalidOperationException();
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => throw new InvalidOperationException();
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new InvalidOperationException();
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => throw new InvalidOperationException();

        [Serializable]
        public struct KeyValuePair
        {
            [SerializeField] private TKey _key;
            [SerializeField] private TValue _value;

            public readonly TKey Key => _key;
            public readonly TValue Value => _value;
        }
    }
}