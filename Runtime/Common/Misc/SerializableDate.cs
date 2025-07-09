using System;
using Newtonsoft.Json;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public struct SerializableDate : IEquatable<SerializableDate>
    {
        [JsonProperty("timestampMs")]
        [SerializeField] private long _timestampMs;

        public static class EditorProps
        {
            public static string TimestampMs => nameof(_timestampMs);
        }

        public static SerializableDate Now => new(DateTimeOffset.Now);
        public static SerializableDate UtcNow => new(DateTimeOffset.UtcNow);

        public readonly DateTimeOffset UtcDate => DateTimeOffset.FromUnixTimeMilliseconds(_timestampMs);
        public readonly DateTimeOffset LocalDate => DateTimeOffset.FromUnixTimeMilliseconds(_timestampMs).ToLocalTime();

        public SerializableDate(DateTimeOffset date) => _timestampMs = date.ToUnixTimeMilliseconds();

        public static implicit operator DateTimeOffset(SerializableDate value) => value.UtcDate;
        public static implicit operator DateTime(SerializableDate value) => value.UtcDate.DateTime;

        public static implicit operator SerializableDate(DateTimeOffset value) => new(value);
        public static implicit operator SerializableDate(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

        public bool Equals(SerializableDate other) => _timestampMs == other._timestampMs;
        public override bool Equals(object obj) => obj is SerializableDate other && Equals(other);

        public override int GetHashCode() => _timestampMs.GetHashCode();

        public static bool operator ==(SerializableDate left, SerializableDate right) => left.Equals(right);
        public static bool operator !=(SerializableDate left, SerializableDate right) => !(left == right);

        public override string ToString() => UtcDate.ToString("o"); // ISO 8601 format, e.g., "2023-10-01T12:34:56.789Z"
    }

}