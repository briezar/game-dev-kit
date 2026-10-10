using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class ArrayExtensions
{
    public static bool Contains<T>(this T[] array, T element) => Array.IndexOf(array, element) >= 0;
    public static bool TrueForAll<T>(this T[] array, Predicate<T> match) => Array.TrueForAll(array, match);
    public static bool Exists<T>(this T[] array, Predicate<T> match) => Array.Exists(array, match);
    public static T Find<T>(this T[] array, Predicate<T> match) => Array.Find(array, match);
    public static bool TryFind<T>(this T[] array, Predicate<T> match, out T result)
    {
        var index = FindIndex(array, match);
        var found = index != -1;
        result = found ? array[index] : default;
        return found;
    }
    public static T[] FindAll<T>(this T[] array, Predicate<T> match) => Array.FindAll(array, match);
    public static int FindIndex<T>(this T[] array, Predicate<T> match) => Array.FindIndex(array, match);
    public static int FindIndex<T>(this T[] array, int startIndex, int count, Predicate<T> match) => Array.FindIndex(array, startIndex, count, match);
    public static int FindIndex<T>(this T[] array, int startIndex, Predicate<T> match) => Array.FindIndex(array, startIndex, match);

    public static int IndexOf<T>(this T[] array, T value, int startIndex, int count) => Array.IndexOf(array, value, startIndex, count);
    public static int IndexOf<T>(this T[] array, T value, int startIndex) => Array.IndexOf(array, value, startIndex);
    public static int IndexOf<T>(this T[] array, T value) => Array.IndexOf(array, value);

    public static void Clear<T>(this T[] array, int? index = null, int? length = null) => Array.Clear(array, index ?? 0, length ?? array.Length);

    public static void Sort<T>(this T[] array) => Array.Sort(array);
    public static void Sort<T>(this T[] array, IComparer<T> comparer) => Array.Sort(array, comparer);
    public static void Sort<T>(this T[] array, Comparison<T> comparison) => Array.Sort(array, comparison);


    /// <summary> Sorts the list from lowest to highest based on the specified key selector. </summary>
    public static void SortAscending<TSource, TKey>(this TSource[] array, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
    {
        array.Sort((a, b) => keySelector(a).CompareTo(keySelector(b)));
    }

    /// <summary> Sorts the list from highest to lowest based on the specified key selector. </summary>
    public static void SortDescending<TSource, TKey>(this TSource[] array, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
    {
        array.Sort((a, b) => keySelector(b).CompareTo(keySelector(a)));
    }

    /// <summary> Sorts the list from highest to lowest. </summary>
    public static void SortDescending<T>(this T[] array) where T : IComparable<T> => array.Sort(static (a, b) => b.CompareTo(a));

    public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array) => Array.AsReadOnly(array);

    public static IEnumerable<(T element, Vector2Int coord)> Enumerate<T>(this T[,] array2D)
    {
        for (int x = 0; x < array2D.GetLength(0); x++)
        {
            for (int y = 0; y < array2D.GetLength(1); y++)
            {
                yield return (array2D[x, y], new Vector2Int(x, y));
            }
        }
    }
}
