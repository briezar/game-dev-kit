using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class CollectionExtension
{
    /// <summary> Remove and return the last element of a list. </summary>
    public static T Pop<T>(this List<T> list)
    {
        if (list.IsNullOrEmpty()) { return default; }
        var lastElem = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        return lastElem;
    }

    /// <summary> Add to the start of a list. </summary>
    public static void Enqueue<T>(this List<T> list, T element)
    {
        if (list == null)
        {
            Debug.LogError($"List is null");
            return;
        }
        list.Insert(0, element);
    }

    /// <summary> Remove and return the first element of a list. </summary>
    public static T Dequeue<T>(this List<T> list)
    {
        if (list.IsNullOrEmpty()) { return default; }
        var firstElem = list[0];
        list.RemoveAt(0);
        return firstElem;
    }

    /// <summary> Returns true if element is successfully added, otherwise false </summary>
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

    /// <summary> Get the first element of a list, null if list has 0 elements. </summary>
    public static T GetFirst<T>(this IList<T> list)
    {
        T firstElem = list.IsNullOrEmpty() ? default : list[0];
        return firstElem;
    }
    /// <summary> Get the last element of a list, null if list has 0 elements. </summary>
    public static T GetLast<T>(this IList<T> list)
    {
        T lastElem = list.IsNullOrEmpty() ? default : list[list.Count - 1];
        return lastElem;
    }

    public static T DequeueWithDefault<T>(this Queue<T> queue, T defaultValue)
    {
        if (queue == null)
        {
            return defaultValue;
        }

        if (queue.Count > 0)
        {
            return queue.Dequeue();
        }

        return defaultValue;
    }

    public static string JoinToString<T>(this IEnumerable<T> enumerable, Func<T, string> selector, string separator = ", ")
    {
        return string.Join(separator, enumerable.Select(selector));
    }
    public static string JoinToString<T>(this IEnumerable<T> enumerable, string separator = ", ")
    {
        return string.Join(separator, enumerable);
    }
    public static string JoinToString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, string format = "{0}:{1}", string separator = "\n")
    {
        if (dictionary == null) { return ""; }
        var result = new List<string>();
        foreach (var item in dictionary)
        {
            result.Add(string.Format(format, item.Key, item.Value));
        }
        return result.JoinToString(separator);
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        return enumerable == null || !enumerable.Any();
    }

    public static T GetRandom<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable.IsNullOrEmpty())
        {
            Debug.Log($"{enumerable} is {(enumerable == null ? "null" : "empty")}");
            return default;
        }

        var list = enumerable as IList<T> ?? enumerable.ToArray();
        return list[Random.Range(0, list.Count)];
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        var count = list.Count;
        for (int i = 0; i < count; i++)
        {
            var temp = list[i];
            var randomIndex = Random.Range(i, count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
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

    public static IEnumerable<(TSource, int)> WithIndex<TSource>(this IEnumerable<TSource> source)
    {
        return source.Select((element, index) => (element, index));
    }

}
