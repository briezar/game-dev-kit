using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public static class EnumerableExtensions
{
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> enumerable) => enumerable ?? Enumerable.Empty<T>();
    public static IEnumerable<T> Empty<T>(this IEnumerable<T> _) => Enumerable.Empty<T>();
    public static T[] EmptyArray<T>(this IEnumerable<T> _) => Array.Empty<T>();

    /// <summary> Returns the symmetric difference (unique elements) of two sequences. </summary>
    public static IEnumerable<T> SymmetricExcept<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        // allocates way less than `first.Except(second).Union(second.Except(first))` and does not scale with collection size
        var diffSet = new HashSet<T>(first);
        diffSet.SymmetricExceptWith(second);
        return diffSet;
    }
    /// <summary>
    /// Returns a pooled list holding a copy of the elements to avoid allocations when iterating IEnumerable.
    /// <br/>Dispose the returned handle to release the list.
    /// </summary>
    public static PooledObject<List<T>> GetListBuffer<T>(this IEnumerable<T> items, out List<T> buffer)
    {
        var pooledObject = ListPool<T>.Get(out buffer);
        buffer.AddRange(items);
        return pooledObject;
    }

    /// <inheritdoc cref="AsCollectionOrPooledCopy"/>
    public static SourceOrPooledCopy<List<T>, T> AsListOrPooledCopy<T>(this IEnumerable<T> items, out List<T> buffer) => new(items, out buffer);

    /// <inheritdoc cref="AsCollectionOrPooledCopy"/>
    public static SourceOrPooledCopy<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> AsDictionaryOrPooledCopy<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items, out Dictionary<TKey, TValue> buffer) => new(items, out buffer);

    /// <inheritdoc cref="AsCollectionOrPooledCopy"/>
    public static SourceOrPooledCopy<HashSet<T>, T> AsHashSetOrPooledCopy<T>(this IEnumerable<T> items, out HashSet<T> buffer) => new(items, out buffer);

    /// <summary> Returns a <see cref="SourceOrPooledCopy{TCollection, TItem}"/> representing a collection that is either the source itself, or a pooled copy. </summary>
    public static SourceOrPooledCopy<TCollection, TItem> AsCollectionOrPooledCopy<TCollection, TItem>(this IEnumerable<TItem> enumerable, out TCollection buffer) where TCollection : class, ICollection<TItem>, new() => new(enumerable, out buffer);

    /// <summary>
    /// Holds a collection that is the source itself when it matches the target type, or a pooled copy otherwise.
    /// <br/>Disposing releases the pooled copy if it is used.
    /// <br/>A null source yields an empty pooled collection.
    /// <br/>E.g. source <c>List&lt;int&gt;</c> with target <c>HashSet&lt;int&gt;</c> holds a pooled <c>HashSet&lt;int&gt;</c>; source <c>HashSet&lt;int&gt;</c> is held as is.
    /// </summary>
    public readonly struct SourceOrPooledCopy<TCollection, TItem> : IDisposable where TCollection : class, ICollection<TItem>, new()
    {
        private readonly TCollection _buffer;
        private readonly bool _isFromPool;

        public SourceOrPooledCopy(IEnumerable<TItem> enumerable, out TCollection buffer)
        {
            if (enumerable is TCollection existing)
            {
                _buffer = buffer = existing;
                _isFromPool = false;
                return;
            }

            CollectionPool<TCollection, TItem>.Get(out var collection);
            try
            {
                Fill(collection, enumerable);
            }
            catch
            {
                CollectionPool<TCollection, TItem>.Release(collection);
                throw;
            }

            _buffer = buffer = collection;
            _isFromPool = true;
        }

        private static void Fill(TCollection collection, IEnumerable<TItem> enumerable)
        {
            if (enumerable == null) { return; }

            if (collection is List<TItem> targetList)
            {
                targetList.AddRange(enumerable);
                return;
            }

            if (enumerable is ICollection<TItem>)
            {
                using var _ = enumerable.GetListBuffer(out var listBuffer);
                foreach (var item in listBuffer)
                {
                    collection.Add(item);
                }
                return;
            }

            foreach (var item in enumerable)
            {
                collection.Add(item);
            }
        }

        public void Dispose()
        {
            if (_isFromPool)
            {
                CollectionPool<TCollection, TItem>.Release(_buffer);
            }
        }
    }

    public static bool HasDuplicates<T>(this IEnumerable<T> source)
    {
        using var _ = HashSetPool<T>.Get(out var set);
        foreach (var item in source)
        {
            if (!set.Add(item))
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        return enumerable switch
        {
            null => true,
            IReadOnlyCollection<T> readOnlyCollection => readOnlyCollection.Count == 0,
            ICollection<T> collection => collection.Count == 0,
            ICollection collection => collection.Count == 0,
            _ => !enumerable.Any(),
        };
    }

    public static T GetRandom<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable.IsNullOrEmpty())
        {
            Debug.Log($"{enumerable} is {(enumerable == null ? "null" : "empty")}");
            return default;
        }

        using var _ = enumerable.GetListBuffer(out var list);
        return list[Random.Range(0, list.Count)];
    }

    public static int IndexOf<T>(this IEnumerable<T> source, T value)
    {
        if (value == null) { return -1; }
        return FindIndex(source, item => item != null && item.Equals(value));
    }

    public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        int index = -1;
        foreach (var item in source)
        {
            index++;
            if (item == null) { continue; }
            if (predicate == null || predicate(item))
            {
                return index;
            }
        }
        return -1;
    }

    public static IEnumerable<(TSource, int)> WithIndex<TSource>(this IEnumerable<TSource> source) => source.Select((e, i) => (e, i));

    public static string JoinToString<T>(this IEnumerable<T> enumerable, Func<T, string> selector, string separator = ", ")
        => string.Join(separator, enumerable.Select(selector));

    public static string JoinToString<T>(this IEnumerable<T> enumerable, string separator = ", ")
        => string.Join(separator, enumerable);

    public static string JoinToString<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dictionary, string format = "{0}:{1}", string separator = "\n")
        => dictionary.Select(pair => string.Format(format, pair.Key, pair.Value)).JoinToString(separator);

}