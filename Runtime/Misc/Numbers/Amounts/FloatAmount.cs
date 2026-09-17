using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public struct FloatAmount<T>
    {
        public T item;
        public float amount;

        public FloatAmount(T item, float amount = 0) => (this.item, this.amount) = (item, amount);
    }

    /// <summary>
    /// Represents a loot table for randomly selecting a FloatAmount item based on weights.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public struct FloatAmountLootTable<T>
    {
        public FloatWeightedAmount<T>[] items;

        public readonly FloatAmount<T> GetRandomItem()
        {
            var randomItem = GeneralUtils.GetWeightedRandom(items, (item) => item.weight);
            return new(randomItem.item, randomItem.amount);
        }
    }

    [Serializable]
    public struct FloatWeightedAmount<T>
    {
        public T item;
        public float amount;
        public float weight;
    }
}