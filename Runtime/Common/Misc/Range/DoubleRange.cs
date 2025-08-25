using System;
using Random = UnityEngine.Random;

namespace GameDevKit
{
    [Serializable]
    public struct DoubleRange
    {
        public double min, max;

        public readonly double Diff => max - min;
        public readonly double AbsDiff => Math.Abs(Diff);

        public DoubleRange(double min, double max) => (this.min, this.max) = (min, max);

        public readonly double GetRandom()
        {
            if (min == max) { return min; }
            var low = Math.Min(min, max);
            var high = Math.Max(min, max);

            return low + (double)Random.value * (high - low) + double.Epsilon; // ensures 'high' can be reached
        }
    }
}