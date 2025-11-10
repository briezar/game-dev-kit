using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameDevKit.Collections
{
    public interface IReadOnlyLookupList<TKey, TValue> : IReadOnlyList<TValue>
    {
        bool TryGet(TKey key, out TValue value);
        TValue Get(TKey key);
    }

    [Serializable]
    public class LookupList<TKey, TValue> : IList<TValue>, IReadOnlyLookupList<TKey, TValue>
    {
        [SerializeField] protected List<TValue> _values = new();

        private readonly Dictionary<TKey, TValue> _dict = new();

        /// <summary>
        /// Must be set or overridden before using the collection.
        /// </summary>
        public virtual Func<TValue, TKey> KeySelector { get; set; }

        public IReadOnlyList<TValue> Values => _values;
        public int Count => _values.Count;

        bool ICollection<TValue>.IsReadOnly => false;

        public TValue this[int index] { get => _values[index]; set => _values[index] = value; }

        protected LookupList() { }
        public LookupList(Func<TValue, TKey> keySelector) => KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        public LookupList(IEnumerable<TValue> elements, Func<TValue, TKey> keySelector)
        {
            _values = elements.ToList();
            KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        }

        /// <summary> Returns true if the value was found, otherwise false. </summary>
        public bool TryGet(TKey key, out TValue value)
        {
            if (_dict.TryGetValue(key, out value)) { return true; }

            foreach (var item in _values)
            {
                if (EqualityComparer<TKey>.Default.Equals(GetKey(item), key))
                {
                    value = item;
                    _dict[key] = item;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary> Gets the value associated with the specified key. </summary>
        /// <exception cref="KeyNotFoundException"></exception>
        public TValue Get(TKey key) => TryGet(key, out var value) ? value : throw new KeyNotFoundException($"The given key '{key}' was not present in the LookupList.");

        public void Add(TValue item)
        {
            if (TryGet(GetKey(item), out _))
            {
                throw new ArgumentException($"An item with the same key '{GetKey(item)}' has already been added.");
            }

            _values.Add(item);
        }

        public void Clear()
        {
            _values.Clear();
            _dict.Clear();
        }

        public bool Contains(TValue item) => _values.Contains(item);
        public bool Remove(TValue item)
        {
            _dict.Remove(GetKey(item));
            return _values.Remove(item);
        }

        private TKey GetKey(TValue item) => KeySelector == null
                ? throw new InvalidOperationException("KeySelector must be set before using the collection.")
                : KeySelector(item);

        void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex) => _values.CopyTo(array, arrayIndex);
        int IList<TValue>.IndexOf(TValue item) => _values.IndexOf(item);
        void IList<TValue>.Insert(int index, TValue item) => _values.Insert(index, item);
        void IList<TValue>.RemoveAt(int index) => _values.RemoveAt(index);

        public List<TValue>.Enumerator GetEnumerator() => _values.GetEnumerator();
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => _values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

    }
}