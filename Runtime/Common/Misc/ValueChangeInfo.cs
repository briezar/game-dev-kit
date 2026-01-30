using System;
using System.Collections;
using System.Collections.Generic;

namespace GameDevKit
{
    [Serializable]
    public struct IntChangeInfo
    {
        public int previous;
        public int current;

        public readonly int Diff => current - previous;

        public IntChangeInfo(int previous, int current) => (this.previous, this.current) = (previous, current);
    }

    [Serializable]
    public struct FloatChangeInfo
    {
        public float previous;
        public float current;

        public readonly float Diff => current - previous;

        public FloatChangeInfo(float previous, float current) => (this.previous, this.current) = (previous, current);
    }
}