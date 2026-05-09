using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public struct IntAmount<T>
    {
        public T item;
        public int amount;

        public IntAmount(T item, int amount) => (this.item, this.amount) = (item, amount);
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