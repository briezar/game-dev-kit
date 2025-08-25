using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public static class ListExtensions
{
    /// <summary> Remove and return the last element of a list. </summary>
    public static T RemoveLast<T>(this List<T> list)
    {
        if (list.IsNullOrEmpty()) { return default; }
        var lastElem = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return lastElem;
    }

    /// <summary> Add to the start of a list. </summary>
    public static void AddFirst<T>(this List<T> list, T element)
    {
        if (list == null)
        {
            Debug.LogError($"List is null");
            return;
        }
        list.Insert(0, element);
    }

    /// <summary> Remove and return the first element of a list. </summary>
    public static T RemoveFirst<T>(this List<T> list)
    {
        if (list.IsNullOrEmpty()) { return default; }
        var firstElem = list[0];
        list.RemoveAt(0);
        return firstElem;
    }

    /// <summary> Returns false if element already exists, otherwise true </summary>
    public static bool TryAdd<T>(this IList<T> list, T element)
    {
        if (list == null)
        {
            Debug.LogError($"List is null");
            return false;
        }

        if (list.Contains(element))
        {
            return false;
        }

        list.Add(element);
        return true;
    }

    public static bool TryGet<T>(this IReadOnlyList<T> list, int index, out T element)
    {
        if (list.IsNullOrEmpty() || index < 0 || index >= list.Count)
        {
            element = default;
            return false;
        }

        element = list[index];
        return true;
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        var count = list.Count;
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(i, count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public static void SortAscending<TSource, TKey>(this List<TSource> list, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
    {
        list.Sort((a, b) => keySelector(a).CompareTo(keySelector(b)));
    }
    public static void SortDescending<TSource, TKey>(this List<TSource> list, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
    {
        list.Sort((a, b) => keySelector(b).CompareTo(keySelector(a)));
    }
    public static void SortDescending<T>(this List<T> list)
    {
        list.Sort();
        list.Reverse();
    }

}
