using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit.Collections
{
    public interface IReadOnlyLookupList<TKey, TValue> : IReadOnlyList<TValue>
    {
        bool TryGet(TKey key, out TValue value);
        TValue Get(TKey key);
        int IndexOfKey(TKey key);
    }

    [Serializable]
    public class LookupList<TKey, TValue> : IList<TValue>, IReadOnlyLookupList<TKey, TValue>, ISerializationCallbackReceiver
    {
        private static readonly EqualityComparer<TKey> Comparer = EqualityComparer<TKey>.Default;

        [SerializeField] protected List<TValue> _values = new();

        private readonly Dictionary<TKey, int> _indices = new();
        private bool _dirty = true;

        /// <summary> Must be set before use, e.g. in the constructor of a concrete subclass. </summary>
        public Func<TValue, TKey> KeySelector { get; set; }

        public IReadOnlyList<TValue> Values => _values;
        public int Count => _values.Count;
        bool ICollection<TValue>.IsReadOnly => false;

        protected LookupList() { }
        public LookupList(Func<TValue, TKey> keySelector) => KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        public LookupList(IEnumerable<TValue> elements, Func<TValue, TKey> keySelector) : this(keySelector)
        {
            foreach (var element in elements ?? throw new ArgumentNullException(nameof(elements))) { Add(element); }
        }

        public TValue this[int index]
        {
            get => _values[index];
            set
            {
                var existing = IndexOfKey(GetKey(value));
                if (existing >= 0 && existing != index) { throw DuplicateKey(GetKey(value)); }

                _values[index] = value;
                _dirty = true; // the key may have changed
            }
        }

        public int IndexOfKey(TKey key)
        {
            if (key is null) { return -1; }

            EnsureIndex();
            if (_indices.TryGetValue(key, out var index) && index < _values.Count && Comparer.Equals(GetKey(_values[index]), key)) { return index; }

            // Missing or stale: the list may have been edited behind our back.
            _dirty = true;
            EnsureIndex();
            return _indices.TryGetValue(key, out index) ? index : -1;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            var index = IndexOfKey(key);
            value = index >= 0 ? _values[index] : default;
            return index >= 0;
        }

        public TValue Get(TKey key) => TryGet(key, out var value) ? value : throw new KeyNotFoundException($"The given key '{key}' was not present in the LookupList.");

        public bool ContainsKey(TKey key) => IndexOfKey(key) >= 0;
        public bool Contains(TValue item) => ContainsKey(GetKey(item));

        public void Add(TValue item)
        {
            var key = GetKey(item);
            if (key is null) { throw new ArgumentException("Key must not be null."); }
            if (IndexOfKey(key) >= 0) { throw DuplicateKey(key); }

            _values.Add(item);
            _indices[key] = _values.Count - 1; // append keeps the map valid
        }

        public void Insert(int index, TValue item)
        {
            var key = GetKey(item);
            if (IndexOfKey(key) >= 0) { throw DuplicateKey(key); }

            _values.Insert(index, item);
            _dirty = true;
        }

        public bool RemoveKey(TKey key)
        {
            var index = IndexOfKey(key);
            if (index < 0) { return false; }

            RemoveAt(index);
            return true;
        }

        public bool Remove(TValue item) => RemoveKey(GetKey(item));

        public void RemoveAt(int index)
        {
            var isLast = index == _values.Count - 1;
            if (isLast && !_dirty)
            {
                var key = GetKey(_values[index]);
                if (key is not null) { _indices.Remove(key); }
            }
            else
            {
                _dirty = true;
            }

            _values.RemoveAt(index);
        }

        public int RemoveAll(Predicate<TValue> match)
        {
            var removed = _values.RemoveAll(match);
            if (removed > 0) { _dirty = true; }

            return removed;
        }

        public void Clear()
        {
            _values.Clear();
            _indices.Clear();
            _dirty = false;
        }

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() => _dirty = true;

        private void EnsureIndex()
        {
            if (!_dirty) { return; }

            _indices.Clear();
            for (var i = 0; i < _values.Count; i++)
            {
                var key = GetKey(_values[i]);
                if (key is null) { continue; }
                _indices.TryAdd(key, i); // first wins; duplicates are an authoring error
            }

            _dirty = false;
        }

        private TKey GetKey(TValue item) => KeySelector == null
            ? throw new InvalidOperationException("KeySelector must be set before using the collection.")
            : KeySelector(item);

        private static ArgumentException DuplicateKey(TKey key) => new($"An item with the same key '{key}' has already been added.");

        void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex) => _values.CopyTo(array, arrayIndex);
        int IList<TValue>.IndexOf(TValue item) => IndexOfKey(GetKey(item));

        public List<TValue>.Enumerator GetEnumerator() => _values.GetEnumerator();
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => _values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
    }
}