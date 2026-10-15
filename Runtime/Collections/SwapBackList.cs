using System;
using System.Collections;
using System.Collections.Generic;

namespace GameDevKit.Collections
{
    /// <summary>
    /// A list-like collection of unique items with O(1) addition, removal and lookup.
    /// <br/>Removal moves the last item into the vacated slot, so order is not preserved.
    /// <br/>E.g. [A, B, C, D] after Remove(B) becomes [A, D, C].
    /// <br/>Items must keep the same hash code and equality while in the collection.
    /// <br/>Adding or removing during a foreach throws <see cref="InvalidOperationException"/>.
    /// <br/>Safely remove items while iterating by reverse looping by index from Count-1 to 0.
    /// <br/>Not thread-safe.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class SwapBackList<T> : IReadOnlyList<T>
    {
        private readonly List<T> _list;
        private readonly Dictionary<T, int> _indexMap;

        public int Count => _list.Count;

        /// <summary> Returns the item at the index. Positions change after <see cref="Remove"/>. </summary>
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
        /// <br/>Returns true if the item was added; false if the item was null or already exists in the collection.
        /// </summary>
        public bool Add(T item)
        {
            if (item == null) { return false; }
            if (!_indexMap.TryAdd(item, _list.Count)) { return false; }

            _list.Add(item);
            return true;
        }

        /// <summary>
        /// Removes an item in O(1) time by moving the last item into its slot.
        /// <br/>Returns true if the item was removed; false if the item was null or not found in the collection.
        /// </summary>
        public bool Remove(T item)
        {
            if (item == null || !_indexMap.TryGetValue(item, out var indexToRemove)) { return false; }

            var lastIndex = _list.Count - 1;

            if (indexToRemove < lastIndex)
            {
                var lastItem = _list[lastIndex];
                _list[indexToRemove] = lastItem;
                _indexMap[lastItem] = indexToRemove;
            }

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

        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}