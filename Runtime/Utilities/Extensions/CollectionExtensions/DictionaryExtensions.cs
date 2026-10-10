using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

public static class DictionaryExtensions
{
    /// <summary>
    /// Returns true when both sequences hold the same keys mapped to equal values, regardless of order.
    /// <br/>A null sequence is treated as empty.
    /// <br/>A source that is not a <see cref="Dictionary{TKey, TValue}"/> is copied into a pooled one, so duplicate keys in it throw <see cref="ArgumentException"/>.
    /// </summary>
    public static bool ContentEquals<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> first, IEnumerable<KeyValuePair<TKey, TValue>> second)
    {
        if (ReferenceEquals(first, second)) { return true; }

        using var firstHandle = first.AsDictionaryOrPooledCopy(out var firstDict);
        using var secondHandle = second.AsDictionaryOrPooledCopy(out var secondDict);

        if (firstDict.Count != secondDict.Count) { return false; }

        var valueComparer = EqualityComparer<TValue>.Default;
        foreach (var (key, value) in firstDict)
        {
            if (secondDict.TryGetValue(key, out var otherValue) && valueComparer.Equals(value, otherValue))
            {
                continue;
            }
            return false;
        }

        return true;
    }

    public static int RemoveWhere<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, bool> predicate)
    {
        using var _ = ListPool<TKey>.Get(out var keysToRemove);
        foreach (var pair in dictionary)
        {
            if (predicate == null || predicate(pair))
            {
                keysToRemove.Add(pair.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            dictionary.Remove(key);
        }
        return keysToRemove.Count;
    }

}