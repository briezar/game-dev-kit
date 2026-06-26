using UnityEngine;

namespace GameDevKit
{
    /// <summary>
    /// Tracks whether a resource is in use using a simple reference-like counter.
    /// </summary>
    public class UsageCounter
    {
        /// <summary>
        /// Gets the current number of active uses.
        /// </summary>
        public int UseCount { get; private set; } = 0;

        /// <summary>
        /// Gets a value indicating whether the counter currently represents an active use.
        /// </summary>
        public bool IsUsing => UseCount > 0;

        /// <summary>
        /// Adjusts the counter and reports whether the overall using state changed.
        /// </summary>
        /// <param name="use">If true, increments the counter; otherwise decrements it.</param>
        /// <param name="usingStateChanged">True when the <see cref="IsUsing"/> state changes as a result of this call.</param>
        /// <returns>True if the counter is currently in use after the change; otherwise false.</returns>
        public bool Use(bool use, out bool usingStateChanged)
        {
            var prev = IsUsing;
            var current = Use(use);
            usingStateChanged = prev != current;
            return current;
        }

        /// <summary>
        /// Adjusts the counter by one step.
        /// </summary>
        /// <param name="use">If true, increments the counter; otherwise decrements it.</param>
        /// <returns>True if the counter is currently in use after the change; otherwise false.</returns>
        public bool Use(bool use)
        {
            UseCount += use ? 1 : -1;
            if (UseCount < 0) { UseCount = 0; }
            return IsUsing;
        }

        /// <summary>
        /// Increments the active use count by one.
        /// </summary>
        public void Increment() => Use(true);

        /// <summary>
        /// Decrements the active use count by one.
        /// </summary>
        /// <returns>True if the counter is currently in use after the decrement; otherwise false.</returns>
        public bool Decrement() => Use(false);

        /// <summary>
        /// Resets the active use count to zero.
        /// </summary>
        public void Reset() => UseCount = 0;
    }
}
