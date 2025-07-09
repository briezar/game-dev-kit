using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameDevKit
{
    [Serializable]
    public struct DoubleRange
    {
        public double min, max;

        public DoubleRange(double min, double max)
        {
            this.min = min;
            this.max = max;
        }

        public readonly double Diff => max - min;
        public readonly double AbsDiff => Math.Abs(max - min);
        public readonly double GetRandom() => min + Random.value * Diff;
    }
}