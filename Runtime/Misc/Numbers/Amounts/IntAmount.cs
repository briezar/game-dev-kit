using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace GameDevKit
{
    [Serializable]
    public struct IntAmount<T>
    {
        public T item;
        public int amount;

        public IntAmount(T item, int amount = 0) => (this.item, this.amount) = (item, amount);

        private static void AssertEqualItem(IntAmount<T> left, IntAmount<T> right)
        {
            Assert.AreEqual(left.item, right.item, $"Cannot operate on amount values with different items: '{left.item}' != '{right.item}'");
        }

        public static IntAmount<T> operator +(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualItem(left, right);
            return new(left.item, left.amount + right.amount);
        }

        public static IntAmount<T> operator -(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualItem(left, right);
            return new(left.item, left.amount - right.amount);
        }

        public static bool operator >(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualItem(left, right);
            return left.amount > right.amount;
        }

        public static bool operator <(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualItem(left, right);
            return left.amount < right.amount;
        }

        public static bool operator >=(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualItem(left, right);
            return left.amount >= right.amount;
        }

        public static bool operator <=(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualItem(left, right);
            return left.amount <= right.amount;
        }

        public static bool operator ==(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualItem(left, right);
            return left.amount == right.amount;
        }

        public static bool operator !=(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualItem(left, right);
            return left.amount != right.amount;
        }

        public override readonly bool Equals(object obj)
        {
            if (obj is IntAmount<T> other)
            {
                return this == other;
            }
            return false;
        }

        public override readonly int GetHashCode() => HashCode.Combine(item, amount);

        public static implicit operator IntAmount<T>((T item, int amount) tuple) => new(tuple.item, tuple.amount);
    }

    /// <summary>
    /// Represents a loot table for randomly selecting an IntAmount item based on weights.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public struct IntAmountLootTable<T>
    {
        [Serializable]
        public struct ItemWeight
        {
            public T Item;
            public int Amount;
            public float Weight;
        }

        public ItemWeight[] Items;

        public readonly IntAmount<T> Roll()
        {
            var randomItem = GeneralUtils.GetWeightedRandom(Items, static (item) => item.Weight);
            return new(randomItem.Item, randomItem.Amount);
        }
    }

    [Serializable]
    public struct IntAmountChance<T>
    {
        public T Item;
        public int Amount;
        [Range(0f, 1f)] public float Chance;
    }

    public static class IntAmountExtensions
    {
        public static bool HasEnough<T>(this IntAmount<T> left, IntAmount<T> right) => left >= right;

        public static bool HasEnough<T>(this IEnumerable<IntAmount<T>> availableItems, IEnumerable<IntAmount<T>> requirements)
        {
            using var _ = DictionaryPool<T, int>.Get(out var amounts);
            foreach (var item in availableItems)
            {
                amounts.TryAdd(item.item, 0);
                amounts[item.item] += item.amount;
            }
            return requirements.All(ingredient => amounts.TryGetValue(ingredient.item, out var amount) && amount >= ingredient.amount);
        }
    }
}