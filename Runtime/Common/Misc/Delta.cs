using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameDevKit
{
    [Serializable]
    public struct IntDelta
    {
        public int previous, current;

        public readonly int Delta => current - previous;

        public IntDelta(int previous, int current)
        {
            this.previous = previous;
            this.current = current;
        }
    }

    [Serializable]
    public struct FloatDelta
    {
        public float previous, current;

        public readonly float Delta => current - previous;

        public FloatDelta(float previous, float current)
        {
            this.previous = previous;
            this.current = current;
        }
    }

    [Serializable]
    public struct Delta<T>
    {
        public T previous, current;

        public Delta(T previous, T current)
        {
            this.previous = previous;
            this.current = current;
        }
    }
}