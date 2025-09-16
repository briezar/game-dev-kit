using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameDevKit.Collections
{
    [Serializable]
    public class ReadOnlyLookupList<TKey, TValue> : IEnumerable<TValue>
    {
        [SerializeField] protected List<TValue> _values = new();

        private Dictionary<TKey, TValue> _dict;

        private const int DictionaryThreshold = 8;

        /// <summary>
        /// Must be set or overridden before using the collection.
        /// </summary>
        public virtual Func<TValue, TKey> KeySelector { get; protected set; }

        protected ReadOnlyLookupList() { }
        public ReadOnlyLookupList(Func<TValue, TKey> keySelector) => KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        public ReadOnlyLookupList(IEnumerable<TValue> elements, Func<TValue, TKey> keySelector)
        {
            _values = elements.ToList();
            KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        }

        public IReadOnlyList<TValue> Values => _values;
        public int Count => _values.Count;

        public virtual void Bind(Func<TValue, TKey> keySelector) => KeySelector = keySelector;

        /// <summary>
        /// Returns true if the value was found, otherwise false.
        /// </summary>
        public bool TryGet(TKey key, out TValue value)
        {
            if (_values.Count > DictionaryThreshold)
            {
                EnsureDictionary();
                return _dict.TryGetValue(key, out value);
            }

            foreach (var item in _values)
            {
                if (EqualityComparer<TKey>.Default.Equals(KeySelector(item), key))
                {
                    value = item;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Returns the value if found, otherwise returns default(TValue).
        /// </summary>
        public TValue Get(TKey key) => TryGet(key, out var value) ? value : default;

        private void EnsureDictionary()
        {
            if (_dict != null) { return; }

            _dict = new(_values.Count);
            foreach (var item in _values)
            {
                _dict[KeySelector(item)] = item;
            }
        }

        public List<TValue>.Enumerator GetEnumerator() => _values.GetEnumerator();
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => _values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
    }
}