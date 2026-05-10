using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameDevKit
{
    [Serializable]
    public struct IntAmount<T>
    {
        public T item;
        public int amount;

        public IntAmount(T item, int amount = 0) => (this.item, this.amount) = (item, amount);

        private static void AssertEqualResource(IntAmount<T> left, IntAmount<T> right)
        {
            Assert.AreEqual(left.item, right.item, $"Cannot operate on amount values with different items: '{left.item}' != '{right.item}'");
        }

        public static IntAmount<T> operator +(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualResource(left, right);
            return new(left.item, left.amount + right.amount);
        }

        public static IntAmount<T> operator -(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualResource(left, right);
            return new(left.item, left.amount - right.amount);
        }

        public static bool operator >(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualResource(left, right);
            return left.amount > right.amount;
        }

        public static bool operator <(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualResource(left, right);
            return left.amount < right.amount;
        }

        public static bool operator >=(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualResource(left, right);
            return left.amount >= right.amount;
        }

        public static bool operator <=(IntAmount<T> left, IntAmount<T> right)
        {
            AssertEqualResource(left, right);
            return left.amount <= right.amount;
        }

    }

    [Serializable]
    public struct IntRandomAmount<T>
    {
        public IntWeightedAmount<T>[] items;

        public readonly IntAmount<T> GetRandomItem()
        {
            var randomItem = GeneralUtils.GetWeightedRandom(items, (item) => item.weight);
            return new(randomItem.item, randomItem.amount);
        }
    }

    [Serializable]
    public struct IntWeightedAmount<T>
    {
        public T item;
        public int amount;
        public float weight;
    }
}