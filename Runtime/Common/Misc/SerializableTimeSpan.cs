using System;
using Newtonsoft.Json;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public struct SerializableTimeSpan : IEquatable<SerializableTimeSpan>, IComparable<SerializableTimeSpan>
    {
        [JsonProperty("durationMs")]
        [SerializeField] private long _durationMs;

#if UNITY_EDITOR
        internal static class EditorProps
        {
            public static string DurationMs => nameof(_durationMs);
        }
#endif

        public static SerializableTimeSpan Zero => new(0);

        public readonly TimeSpan TimeSpan => TimeSpan.FromMilliseconds(_durationMs);

        public SerializableTimeSpan(long milliseconds) => _durationMs = milliseconds;
        public SerializableTimeSpan(TimeSpan timeSpan) => _durationMs = (long)timeSpan.TotalMilliseconds;

        public static SerializableTimeSpan FromMilliseconds(double value) => TimeSpan.FromMilliseconds(value);
        public static SerializableTimeSpan FromSeconds(double value) => TimeSpan.FromSeconds(value);
        public static SerializableTimeSpan FromMinutes(double value) => TimeSpan.FromMinutes(value);
        public static SerializableTimeSpan FromHours(double value) => TimeSpan.FromHours(value);
        public static SerializableTimeSpan FromDays(double value) => TimeSpan.FromDays(value);

        public static implicit operator TimeSpan(SerializableTimeSpan value) => value.TimeSpan;
        public static implicit operator SerializableTimeSpan(TimeSpan value) => new(value);

        public readonly bool Equals(SerializableTimeSpan other) => _durationMs == other._durationMs;
        public override readonly bool Equals(object obj) => obj is SerializableTimeSpan other && Equals(other);

        public override readonly int GetHashCode() => _durationMs.GetHashCode();

        public readonly int CompareTo(SerializableTimeSpan other) => _durationMs.CompareTo(other._durationMs);

        public static bool operator ==(SerializableTimeSpan left, SerializableTimeSpan right) => left.Equals(right);
        public static bool operator !=(SerializableTimeSpan left, SerializableTimeSpan right) => !(left == right);
        public static bool operator <(SerializableTimeSpan left, SerializableTimeSpan right) => left._durationMs < right._durationMs;
        public static bool operator <=(SerializableTimeSpan left, SerializableTimeSpan right) => left._durationMs <= right._durationMs;
        public static bool operator >(SerializableTimeSpan left, SerializableTimeSpan right) => left._durationMs > right._durationMs;
        public static bool operator >=(SerializableTimeSpan left, SerializableTimeSpan right) => left._durationMs >= right._durationMs;

        public static SerializableTimeSpan operator +(SerializableTimeSpan left, SerializableTimeSpan right) => new(left._durationMs + right._durationMs);
        public static SerializableTimeSpan operator -(SerializableTimeSpan left, SerializableTimeSpan right) => new(left._durationMs - right._durationMs);
        public static SerializableTimeSpan operator *(SerializableTimeSpan timeSpan, double factor) => new((long)(timeSpan._durationMs * factor));
        public static SerializableTimeSpan operator /(SerializableTimeSpan timeSpan, double divisor) => new((long)(timeSpan._durationMs / divisor));

        public override readonly string ToString()
        {
            var duration = TimeSpan;
            if (duration.TotalDays > 0)
            {
                return $"{duration:d'd 'h'h 'm'm 's's'}";
            }
            if (duration.TotalHours > 0)
            {
                return $"{duration:h 'h 'm'm 's's'}";
                // timeText = $"{h}h {m}m";
            }
            return $"{duration:m'm 's's.'ff}";
        }
    }
}
