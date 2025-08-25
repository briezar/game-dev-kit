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
        return first.Except(second).Union(second.Except(first));
    }

    public static bool HasDuplicates<T>(this IEnumerable<T> source)
    {
        using var _ = HashSetPool<T>.Get(out var set);
        foreach (var item in source)
        {
            if (!set.Add(item))
            {
                return true; // Duplicate found
            }
        }
        return false; // No duplicates found
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null) { return true; }
        return enumerable switch
        {
            IReadOnlyCollection<T> readOnlyCollection => readOnlyCollection.Count == 0,
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

        using var _ = ListPool<T>.Get(out var list);
        list.AddRange(enumerable);
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

    public static bool TryGet<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, out T element)
    {
        element = default;
        if (enumerable.IsNullOrEmpty() || predicate == null) { return false; }

        foreach (var item in enumerable)
        {
            if (predicate(item))
            {
                element = item;
                return true;
            }
        }

        return false;
    }

    public static IEnumerable<(TSource, int)> WithIndex<TSource>(this IEnumerable<TSource> source) => source.Select((e, i) => (e, i));

    public static string JoinToString<T>(this IEnumerable<T> enumerable, Func<T, string> selector, string separator = ", ")
        => string.Join(separator, enumerable.Select(selector));

    public static string JoinToString<T>(this IEnumerable<T> enumerable, string separator = ", ")
        => string.Join(separator, enumerable);

    public static string JoinToString<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dictionary, string format = "{0}:{1}", string separator = "\n")
        => dictionary.Select(pair => string.Format(format, pair.Key, pair.Value)).JoinToString(separator);

}