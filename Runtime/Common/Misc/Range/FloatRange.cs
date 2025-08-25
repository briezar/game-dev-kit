using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameDevKit
{
    [Serializable]
    public struct FloatRange
    {
        public float min, max;

        public readonly float Diff => max - min;
        public readonly float AbsDiff => Mathf.Abs(Diff);

        public FloatRange(float min, float max) => (this.min, this.max) = (min, max);

        public readonly float GetRandom() => Random.Range(min, max);
    }
}