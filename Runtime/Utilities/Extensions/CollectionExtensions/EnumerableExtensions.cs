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
    /// Gets a pooled list buffer containing the elements of the enumerable to avoid allocations when iterating IEnumerable.
    /// Call Dispose on the returned PooledObject to release the buffer back to the pool, or use a using statement to automatically release it.
    /// </summary>
    public static PooledObject<List<T>> GetListBuffer<T>(this IEnumerable<T> items, out List<T> buffer)
    {
        var pooledObject = ListPool<T>.Get(out buffer);
        buffer.AddRange(items);
        return pooledObject;
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