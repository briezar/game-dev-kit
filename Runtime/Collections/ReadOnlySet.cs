using System;
using System.Collections;
using System.Collections.Generic;

namespace GameDevKit.Collections
{
    public interface IReadOnlySet<T> : IReadOnlyCollection<T>
    {
        bool Contains(T i);
        bool IsProperSubsetOf(IEnumerable<T> other);
        bool IsProperSupersetOf(IEnumerable<T> other);
        bool IsSubsetOf(IEnumerable<T> other);
        bool IsSupersetOf(IEnumerable<T> other);
        bool Overlaps(IEnumerable<T> other);
        bool SetEquals(IEnumerable<T> other);
        void CopyTo(T[] array, int arrayIndex);
    }

    public class ReadOnlyHashSet<T> : ReadOnlySet<T>
    {
        public ReadOnlyHashSet(IEnumerable<T> values) : this(new HashSet<T>(values)) { }
        public ReadOnlyHashSet(params T[] values) : this(new HashSet<T>(values)) { }
        public ReadOnlyHashSet(HashSet<T> set) : base(set) { }

        public HashSet<T>.Enumerator GetEnumerator() => ((HashSet<T>)_set).GetEnumerator();
    }

    public class ReadOnlySet<T> : IReadOnlySet<T>, ISet<T>
    {
        protected readonly ISet<T> _set;

        public int Count => _set.Count;
        bool ICollection<T>.IsReadOnly => true;

        public ReadOnlySet(ISet<T> set)
        {
            _set = set;
        }

        public bool Contains(T i) => _set.Contains(i);
        public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);
        public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);
        public void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _set.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _set.GetEnumerator();

        bool ISet<T>.Add(T item) => throw new NotSupportedException();
        void ISet<T>.ExceptWith(IEnumerable<T> other) => throw new NotSupportedException();
        void ISet<T>.IntersectWith(IEnumerable<T> other) => throw new NotSupportedException();
        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException();
        void ISet<T>.UnionWith(IEnumerable<T> other) => throw new NotSupportedException();
        void ICollection<T>.Add(T item) => throw new NotSupportedException();
        void ICollection<T>.Clear() => throw new NotSupportedException();
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
    }

    public static class HashSetExtensions
    {
        public static ReadOnlyHashSet<T> AsReadOnly<T>(this HashSet<T> set) => new(set);
    }
}