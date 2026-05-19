using System;
using System.Collections;
using System.Collections.Generic;

namespace GameDevKit
{
    [Serializable]
    public struct ValueChangeInfo<T>
    {
        public T previous;
        public T current;

        public ValueChangeInfo(T previous, T current) => (this.previous, this.current) = (previous, current);
        public ValueChangeInfo(T current) => (this.previous, this.current) = (current, current);
    }

    [Serializable]
    public struct IntChangeInfo
    {
        public int previous;
        public int current;

        public readonly int Diff => current - previous;

        public IntChangeInfo(int previous, int current) => (this.previous, this.current) = (previous, current);
        public IntChangeInfo(int current) => (this.previous, this.current) = (current, current);
    }

    [Serializable]
    public struct FloatChangeInfo
    {
        public float previous;
        public float current;

        public readonly float Diff => current - previous;

        public FloatChangeInfo(float previous, float current) => (this.previous, this.current) = (previous, current);
        public FloatChangeInfo(float current) => (this.previous, this.current) = (current, current);
    }
}