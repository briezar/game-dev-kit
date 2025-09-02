using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

public static class DictionaryExtensions
{
    public static bool ContainsKey<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable, TKey key)
    {
        return enumerable switch
        {
            IDictionary<TKey, TValue> dict => dict.ContainsKey(key),
            IReadOnlyDictionary<TKey, TValue> dict => dict.ContainsKey(key),
            _ => enumerable.Any(pair => pair.Key.Equals(key)),
        };
    }

    public static TValue GetValue<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable, TKey key)
    {
        return enumerable switch
        {
            IDictionary<TKey, TValue> dict => dict[key],
            IReadOnlyDictionary<TKey, TValue> dict => dict[key],
            _ => enumerable.FirstOrDefault(pair => pair.Key.Equals(key)).Value,
        };
    }

    public static TValue GetOrCreateValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory = null)
    {
        if (dictionary.TryGetValue(key, out var value)) { return value; }
        if (valueFactory == null) { return value; }

        value = valueFactory();
        dictionary[key] = value;
        return value;
    }

    public static bool TryGetValue<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable, TKey key, out TValue value)
    {
        switch (enumerable)
        {
            case IDictionary<TKey, TValue> dict:
                return dict.TryGetValue(key, out value);
            case IReadOnlyDictionary<TKey, TValue> dict:
                return dict.TryGetValue(key, out value);
            default:
                foreach (var pair in enumerable)
                {
                    if (pair.Key.Equals(key))
                    {
                        value = pair.Value;
                        return true;
                    }
                }
                value = default;
                return false;
        }
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