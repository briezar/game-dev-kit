using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameDevKit
{
    [Serializable]
    public struct FloatRange
    {
        public float min, max;

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public readonly float Diff => max - min;
        public readonly float AbsDiff => Mathf.Abs(max - min);
        public readonly float GetRandom() => Random.Range(min, max);
    }
}