using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameDevKit
{
    [Serializable]
    public struct IntRange
    {
        public int min, max;

        public readonly int Diff => max - min;
        public readonly float AbsDiff => Mathf.Abs(Diff);

        public IntRange(int min, int max) => (this.min, this.max) = (min, max);

        public readonly int GetRandom(bool inclusive = true) => Random.Range(min, inclusive ? max + 1 : max);
    }
}