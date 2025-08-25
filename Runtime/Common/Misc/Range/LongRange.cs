using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameDevKit
{
    [Serializable]
    public struct LongRange
    {
        public long min, max;

        public readonly long Diff => max - min;
        public readonly long AbsDiff => Math.Abs(Diff);

        public LongRange(long min, long max) => (this.min, this.max) = (min, max);

        public readonly long GetRandom(bool inclusive = true)
        {
            if (min == max) { return min; }
            var low = Math.Min(min, max);
            var high = Math.Max(min, max);

            var range = high - low + (inclusive ? 1 : 0);
            var offset = (double)Random.value * range + double.Epsilon;
            return low + (long)offset;
        }
    }
}