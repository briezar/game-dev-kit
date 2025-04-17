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

        public AutoCountdownTimer(TimeSpan duration) : this(duration, duration) { }
        public AutoCountdownTimer(TimeSpan currentDuration, TimeSpan duration)
        {
            currentDuration = currentDuration.Clamp(TimeSpan.Zero, duration);
            _cooldownTimestamp = Time.time + (float)currentDuration.TotalSeconds;
            Duration = duration;
        }

        public void Restart()
        {
            _cooldownTimestamp = Time.time + (float)Duration.TotalSeconds;
        }

        public void Complete()
        {
            _cooldownTimestamp = Time.time;
        }
    }

    public struct ManualCountdownTimer
    {
        public TimeSpan Current { get; private set; }
        public TimeSpan Duration { get; private set; }
        public readonly bool IsCompleted => Current <= TimeSpan.Zero;

        public ManualCountdownTimer(TimeSpan duration) : this(duration, duration) { }
        public ManualCountdownTimer(TimeSpan current, TimeSpan duration)
        {
            current = current.ClampMax(duration);

            Current = current;
            Duration = duration;
        }

        public bool TickAndCheck(float? interval = null)
        {
            Tick(interval);
            return IsCompleted;
        }

        public void Tick(float? interval = null)
        {
            if (Current < TimeSpan.Zero) { return; }
            var intervalTimeSpan = TimeSpan.FromSeconds(interval != null ? interval.Value : Time.deltaTime);
            Current -= intervalTimeSpan;
            Current = Current.ClampMin(TimeSpan.Zero);
        }

        public void Restart()
        {
            Current = Duration;
        }
    }
}