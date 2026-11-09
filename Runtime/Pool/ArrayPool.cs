using System;
using System.Runtime.CompilerServices;

namespace GameDevKit.Pool
{
    public static class ArrayPool<T>
    {
        private static readonly System.Buffers.ArrayPool<T> _pool = System.Buffers.ArrayPool<T>.Shared;
        private static readonly bool _clearByDefault = RuntimeHelpers.IsReferenceOrContainsReferences<T>();

        /// <summary>
        /// Rents an array and returns it to the pool when the scope is disposed.
        /// <br/>Contents are not cleared on rent.
        /// <br/>A null <paramref name="clearOnRelease"/> clears only when <typeparamref name="T"/> holds references.
        /// </summary>
        public static PoolScope Get(int minLength, out T[] value, bool? clearOnRelease = null) => new(minLength, clearOnRelease ?? _clearByDefault, out value);

        /// <summary>
        /// Rents an array with at least <paramref name="minLength"/> elements.
        /// <br/>E.g. minLength 100 can return an array of length 128.
        /// </summary>
        public static T[] Get(int minLength) => _pool.Rent(minLength);

        /// <summary> Returns the array to the pool. Must not be released twice. </summary>
        public static void Release(T[] arr, bool? clearArray = null) => _pool.Return(arr, clearArray ?? _clearByDefault);

        public readonly struct PoolScope : IDisposable
        {
            private readonly T[] _array;
            private readonly bool _clearArray;

            public PoolScope(int minLength, bool clearArray, out T[] value)
            {
                _array = value = _pool.Rent(minLength);
                _clearArray = clearArray;
            }

            public void Dispose() => _pool.Return(_array, _clearArray);
        }
    }
}