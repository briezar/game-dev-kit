using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ListExtensions
{
    /// <summary> Remove and return the last element of a list matching the predicate. </summary>
    public static T RemoveLast<T>(this IList<T> list, Func<T, bool> predicate = null)
    {
        if (list.IsNullOrEmpty()) { return default; }
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (predicate == null || predicate(list[i]))
            {
                var elem = list[i];
                list.RemoveAt(i);
                return elem;
            }
        }
        return default;
    }

    /// <summary> Add to the start of a list. </summary>
    public static void AddFirst<T>(this IList<T> list, T element)
    {
        if (list == null)
        {
            Debug.LogError($"List is null");
            return;
        }
        list.Insert(0, element);
    }

    /// <summary> Remove and return the first element of a list matching the predicate. </summary>
    public static T RemoveFirst<T>(this IList<T> list, Func<T, bool> predicate = null)
    {
        if (list.IsNullOrEmpty()) { return default; }
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate == null || predicate(list[i]))
            {
                var elem = list[i];
                list.RemoveAt(i);
                return elem;
            }
        }
        return default;
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

    public static int FindIndex<T>(this IReadOnlyList<T> list, Func<T, bool> predicate) => list.TryFindIndex(predicate, out var index) ? index : -1;
    public static bool TryFindIndex<T>(this IReadOnlyList<T> list, Func<T, bool> predicate, out int index)
    {
        index = -1;
        if (list.IsNullOrEmpty() || predicate == null) { return false; }

        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i]))
            {
                index = i;
                return true;
            }
        }

        return false;
    }

    public static T Find<TState, T>(this IReadOnlyList<T> list, TState state, Func<TState, T, bool> match)
    {
        if (list.IsNullOrEmpty() || match == null) { return default; }
        for (int i = 0; i < list.Count; i++)
        {
            if (match(state, list[i])) { return list[i]; }
        }
        return default;
    }

    public static bool TryFind<T>(this IReadOnlyList<T> list, Func<T, bool> predicate, out T element)
    {
        element = default;
        if (list.IsNullOrEmpty() || predicate == null) { return false; }

        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (predicate(item))
            {
                element = item;
                return true;
            }
        }

        return false;
    }

    public static bool TryFind<T>(this IReadOnlyList<T> list, int index, out T element)
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
    /// <summary> Sorts the list from lowest to highest based on the specified key selector. </summary>
    public static void SortAscending<TSource, TKey>(this List<TSource> list, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
    {
        list.Sort((a, b) => keySelector(a).CompareTo(keySelector(b)));
    }

    /// <summary>
    /// Sorts the list from lowest to highest based on the specified key selector.
    /// <br/>Pass a non-capturing <c>static</c> selector and the state for 0 allocation sorting.
    /// </summary>
    public static void SortAscending<TState, TSource, TKey>(this List<TSource> list, TState state, Func<TState, TSource, TKey> keySelector) where TKey : IComparable<TKey>
        => SortWithState(list, state, keySelector, true);

    /// <summary> Sorts the list from highest to lowest based on the specified key selector. </summary>
    public static void SortDescending<TSource, TKey>(this List<TSource> list, Func<TSource, TKey> keySelector) where TKey : IComparable<TKey>
    {
        list.Sort((a, b) => keySelector(b).CompareTo(keySelector(a)));
    }

    /// <summary>
    /// Sorts the list from highest to lowest based on the specified key selector.
    /// <br/>Pass a non-capturing <c>static</c> selector and the state for 0 allocation sorting.
    /// </summary>
    public static void SortDescending<TState, TSource, TKey>(this List<TSource> list, TState state, Func<TState, TSource, TKey> keySelector) where TKey : IComparable<TKey>
        => SortWithState(list, state, keySelector, false);

    /// <summary> Sorts the list from highest to lowest. </summary>
    public static void SortDescending<T>(this List<T> list) where T : IComparable<T> => list.Sort(static (a, b) => b.CompareTo(a));

    private static void SortWithState<TState, TSource, TKey>(List<TSource> list, TState state, Func<TState, TSource, TKey> keySelector, bool ascending) where TKey : IComparable<TKey>
    {
        using var comparer = StateComparer<TState, TSource, TKey>.Rent(state, keySelector, ascending);
        list.Sort(comparer);
    }

    private sealed class StateComparer<TState, TSource, TKey> : IComparer<TSource>, IDisposable where TKey : IComparable<TKey>
    {
        [ThreadStatic] private static StateComparer<TState, TSource, TKey> _cached;

        private TState _state;
        private Func<TState, TSource, TKey> _keySelector;
        private bool _ascending;

        public static StateComparer<TState, TSource, TKey> Rent(TState state, Func<TState, TSource, TKey> keySelector, bool ascending)
        {
            var comparer = _cached ?? new StateComparer<TState, TSource, TKey>();
            _cached = null;
            comparer._state = state;
            comparer._keySelector = keySelector;
            comparer._ascending = ascending;
            return comparer;
        }

        public void Dispose()
        {
            _state = default;
            _keySelector = null;
            _cached = this;
        }

        public int Compare(TSource x, TSource y)
        {
            return _ascending
                ? _keySelector(_state, x).CompareTo(_keySelector(_state, y))
                : _keySelector(_state, y).CompareTo(_keySelector(_state, x));
        }
    }

}
