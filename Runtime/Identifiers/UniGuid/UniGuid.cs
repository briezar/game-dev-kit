using System;
using UnityEngine;

namespace GameDevKit.Identifiers
{
    /// <summary>
    /// Represents a globally unique identifier (GUID) that is serializable with Unity and usable in game scripts.
    /// </summary>
    // https://github.com/adammyhre/Unity-Inventory-System/blob/master/Assets/_Project/Scripts/Inventory/Helpers/SerializableGuid.cs
    [Serializable]
    public struct UniGuid : IEquatable<UniGuid>, IEquatable<Guid>
    {
        [SerializeField] private uint a;
        [SerializeField] private uint b;
        [SerializeField] private uint c;
        [SerializeField] private uint d;

        public static UniGuid Empty => new(0, 0, 0, 0);

        public UniGuid(uint a, uint b, uint c, uint d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public UniGuid(Guid guid)
        {
            var bytes = guid.ToByteArray();
            a = BitConverter.ToUInt32(bytes, 0);
            b = BitConverter.ToUInt32(bytes, 4);
            c = BitConverter.ToUInt32(bytes, 8);
            d = BitConverter.ToUInt32(bytes, 12);
        }

        public static UniGuid NewGuid() => new(Guid.NewGuid());

        public Guid ToGuid()
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(a).CopyTo(bytes, 0);
            BitConverter.GetBytes(b).CopyTo(bytes, 4);
            BitConverter.GetBytes(c).CopyTo(bytes, 8);
            BitConverter.GetBytes(d).CopyTo(bytes, 12);
            return new Guid(bytes);
        }

        public override string ToString() => $"{a:X8}{b:X8}{c:X8}{d:X8}";

        public string ToShortenedString()
        {
            var aa = a.ToString().Length;
            var bb = b.ToString().Length;
            var cc = c.ToString().Length;
            var dd = d.ToString().Length;

            return $"{aa}-{bb}-{cc}-{dd}";
        }


        public override bool Equals(object obj)
        {
            return obj switch
            {
                UniGuid uniGuid => Equals(uniGuid),
                Guid guid => Equals(guid),
                _ => false,
            };
        }

        public bool Equals(UniGuid other) => a == other.a && b == other.b && c == other.c && d == other.d;
        public bool Equals(Guid other) => this == new UniGuid(other);

        public override int GetHashCode() => HashCode.Combine(a, b, c, d);

        public static bool operator ==(UniGuid left, UniGuid right) => left.Equals(right);
        public static bool operator !=(UniGuid left, UniGuid right) => !(left == right);

        public static implicit operator UniGuid(Guid guid) => new(guid);
        public static implicit operator Guid(UniGuid uniGuid) => uniGuid.ToGuid();

    }
}