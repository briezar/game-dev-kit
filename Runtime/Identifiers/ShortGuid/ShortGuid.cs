using System;
using UnityEngine;

namespace GameDevKit.Identifiers
{
    /// <summary>
    /// Serializable wrapper for System.Guid.
    /// Can be implicitly converted to/from System.Guid.
    /// </summary>
    [Serializable]
    public struct ShortGuid : IEquatable<ShortGuid>, IEquatable<Guid>
    {
        [SerializeField] private string _value;

        public static readonly ShortGuid Empty = new(Guid.Empty);

        private Guid? _guidCached;
        private Guid guid
        {
            get
            {
                if (_guidCached == null)
                {
                    if (Guid.TryParse(_value, out var guid))
                    {
                        _guidCached = guid;
                    }
                    else
                    {
                        _guidCached = new();
                        _value = "";
                    }
                }
                return _guidCached.Value;
            }
        }

        public ShortGuid(string value)
        {
            if (!IsValidShortGuid(value))
            {
                Debug.LogWarning($"Attempted to parse an invalid string: {value}. Assigned an empty Guid.");
                this = Empty;
                return;
            }

            _value = value;
            _guidCached = Decode(value);
        }

        public ShortGuid(Guid guid)
        {
            _guidCached = guid;
            _value = Encode(guid);
        }

        public override bool Equals(object obj)
        {
            return obj switch
            {
                ShortGuid serializableGuid => Equals(serializableGuid),
                Guid guid => Equals(guid),
                _ => false,
            };
        }

        public bool Equals(Guid other) => guid.Equals(other);
        public bool Equals(ShortGuid other) => guid.Equals(other.guid);

        public override int GetHashCode() => guid.GetHashCode();
        public override string ToString() => guid.ToString();

        public static ShortGuid NewGuid() => new(Guid.NewGuid());

        public static string Encode(Guid guid)
        {
            string encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded
                .Replace("/", "_")
                .Replace("+", "-");
            return encoded[..22];
        }

        public static Guid Decode(string value)
        {
            value = value
                .Replace("_", "/")
                .Replace("-", "+");
            var buffer = Convert.FromBase64String(value + "==");
            return new Guid(buffer);
        }

        public static bool IsValidShortGuid(string value)
        {
            return !value.IsNullOrEmpty() && value.Length == 22;
        }

        public static bool operator ==(ShortGuid a, ShortGuid b) => a.guid == b.guid;
        public static bool operator !=(ShortGuid a, ShortGuid b) => a.guid != b.guid;
        public static implicit operator ShortGuid(Guid guid) => new(guid);
        public static implicit operator Guid(ShortGuid serializable) => serializable.guid;

    }
}