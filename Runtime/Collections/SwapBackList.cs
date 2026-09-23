using System;
using System.Collections;
using System.Collections.Generic;

namespace GameDevKit.Collections
{
    /// <summary>
    /// A list-like collection that allows for O(1) addition and removal of items using the swap-back pattern.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class SwapBackList<T> : IEnumerable<T>
    {
        private readonly List<T> _list;
        private readonly Dictionary<T, int> _indexMap;

        public int Count => _list.Count;

        public T this[int index] => _list[index];

        public SwapBackList() : this(32, null) { }
        public SwapBackList(IEqualityComparer<T> comparer) : this(32, comparer) { }
        public SwapBackList(int initialCapacity, IEqualityComparer<T> comparer)
        {
            _list = new List<T>(initialCapacity);
            _indexMap = new Dictionary<T, int>(initialCapacity, comparer);
        }

        /// <summary>
        /// Adds an item to the end of the collection in O(1) time.
        /// Returns true if the item was added successfully; false if the item was null or already exists in the collection.
        /// </summary>
        public bool Add(T item)
        {
            if (item == null) { return false; }
            if (!_indexMap.TryAdd(item, _list.Count)) { return false; }

            _list.Add(item);
            return true;
        }

        /// <summary>
        /// Removes an item instantly via the swap-back pattern in O(1) time.
        /// Returns true if the item was removed successfully; false if the item was null or not found in the collection.
        /// </summary>
        public bool Remove(T item)
        {
            if (item == null || !_indexMap.TryGetValue(item, out var indexToRemove)) { return false; }

            var lastIndex = _list.Count - 1;

            // If the item isn't already the last element, swap it
            if (indexToRemove < lastIndex)
            {
                var lastItem = _list[lastIndex];
                _list[indexToRemove] = lastItem;
                _indexMap[lastItem] = indexToRemove;
            }

            // Pop the tail element off
            _list.RemoveAt(lastIndex);
            _indexMap.Remove(item);
            return true;
        }

        public bool Contains(T item) => item != null && _indexMap.ContainsKey(item);

        public void Clear()
        {
            _list.Clear();
            _indexMap.Clear();
        }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}