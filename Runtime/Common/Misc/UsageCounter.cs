using UnityEngine;

namespace GameDevKit
{
    public class UsageCounter
    {
        public int UseCount { get; private set; } = 0;

        public bool IsUsing => UseCount > 0;

        public bool Use(bool use, out bool usingStateChanged)
        {
            var prev = IsUsing;
            var current = Use(use);
            usingStateChanged = prev != current;
            return current;
        }
        public bool Use(bool use)
        {
            UseCount += use ? 1 : -1;
            if (UseCount < 0) { UseCount = 0; }
            return IsUsing;
        }

        public void Increment() => Use(true);
        public bool Decrement() => Use(false);

        public void Reset() => UseCount = 0;
    }
}
