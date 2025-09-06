using System;
using UnityEngine;

namespace GameDevKit
{
    public struct AutoCountdownTimer
    {
        public TimeSpan Duration { get; private set; }
        public readonly TimeSpan Current => TimeSpan.FromSeconds(_cooldownTimestamp - Time.time).Clamp(TimeSpan.Zero, Duration);
        public readonly bool IsCompleted => _cooldownTimestamp <= Time.time;

        private float _cooldownTimestamp;

        public AutoCountdownTimer(TimeSpan duration) : this(duration, TimeSpan.Zero) { }
        public AutoCountdownTimer(TimeSpan currentDuration, TimeSpan duration)
        {
            currentDuration = currentDuration.Clamp(TimeSpan.Zero, duration);
            _cooldownTimestamp = Time.time + (float)currentDuration.TotalSeconds;
            Duration = duration;
        }

        public static AutoCountdownTimer FromSeconds(float seconds) => new(TimeSpan.FromSeconds(seconds));

        public void Complete() => _cooldownTimestamp = Time.time;
        public void Restart() => _cooldownTimestamp = Time.time + (float)Duration.TotalSeconds;
    }

    public struct ManualCountdownTimer
    {
        public TimeSpan Current { get; private set; }
        public TimeSpan Duration { get; private set; }
        public readonly bool IsCompleted => Current <= TimeSpan.Zero;

        public bool CanTick { get; set; }

        /// <summary> Initializes a new timer with current duration set to 0 or full duration based on <paramref name="isCompleted"/> </summary>
        public ManualCountdownTimer(TimeSpan duration, bool isCompleted = true) : this(duration, isCompleted ? TimeSpan.Zero : duration) { }

        /// <summary> Initializes a new timer with <paramref name="duration"/> and current duration set to a lerp between 0 and full duration based on <paramref name="t"/> </summary>
        public ManualCountdownTimer(TimeSpan duration, float t) : this(duration, TimeSpan.FromSeconds(Mathf.Lerp(0, (float)duration.TotalSeconds, t))) { }

        /// <summary> Initializes a new timer with <paramref name="duration"/> and <paramref name="current"/> duration </summary>
        public ManualCountdownTimer(TimeSpan duration, TimeSpan current)
        {
            current = current.ClampMax(duration);

            Current = current;
            Duration = duration;
            CanTick = true;
        }

        public static ManualCountdownTimer FromSeconds(float seconds, float t = 0) => new(TimeSpan.FromSeconds(seconds), t);

        public bool TickAndCheckCompletion(float? interval = null)
        {
            Tick(interval);
            return IsCompleted;
        }

        public void Tick(float? interval = null)
        {
            if (!CanTick) { return; }
            if (Current < TimeSpan.Zero) { return; }
            var intervalTimeSpan = TimeSpan.FromSeconds(interval ?? Time.deltaTime);
            Current -= intervalTimeSpan;
            Current = Current.ClampMin(TimeSpan.Zero);
        }

        public void Complete() => Current = TimeSpan.Zero;
        public void Restart() => Current = Duration;
    }
}