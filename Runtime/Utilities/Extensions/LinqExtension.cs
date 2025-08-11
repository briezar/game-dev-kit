using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

public static class LinqExtension
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

}