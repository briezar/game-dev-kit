using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameDevKit.Identifiers
{
    [TypeConverter(typeof(ShortIdTypeConverter))]
    [JsonConverter(typeof(ShortIdToStringConverter))]
    [Serializable]
    public struct ShortId : IEquatable<ShortId>
    {
        public long value;

        public static ShortId Empty => new(0);

        public ShortId(long id) => value = id;
        public ShortId(string encodedId) => this = Decode(encodedId);

        private static readonly SnowflakeIdGenerator _generator = new(new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds());

        public static ShortId NewId() => new(_generator.GetNextKey());

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/ShortId/Generate Id")]
        private static void GenerateId()
        {
            Debug.Log(NewId());
        }
#endif

        public static string Encode(ShortId shortId)
        {
            return FastEncoder.Base57.Encode((ulong)shortId.value);
        }

        public static ShortId Decode(string encodedString)
        {
            return new((long)FastEncoder.Base57.Decode(encodedString));
        }

        public override readonly int GetHashCode() => value.GetHashCode();
        public override readonly string ToString() => Encode(this);

        public bool Equals(ShortId other) => this == other;
        public override readonly bool Equals(object obj) => obj is ShortId other && this == other;

        public static bool operator ==(ShortId a, ShortId b) => a.value == b.value;
        public static bool operator !=(ShortId a, ShortId b) => !(a == b);

        private class ShortIdToStringConverter : JsonConverter<ShortId>
        {
            public override void WriteJson(JsonWriter writer, ShortId value, JsonSerializer serializer)
            {
                writer.WriteValue(Encode(value));
            }

            public override ShortId ReadJson(JsonReader reader, Type objectType, ShortId existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var encodedString = (string)reader.Value;
                return Decode(encodedString);
            }
        }

        private class ShortIdTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                return value is string stringValue ? Decode(stringValue) : base.ConvertFrom(context, culture, value);
            }
        }

    }

    /// <summary> Copied from UnityEngine.Localization.Tables.DistributedUIDGenerator </summary>
    public class SnowflakeIdGenerator
    {
        // Configured machine id - 10 bits (gives us up to 1024 machines)
        private const int MachineIdBits = 10;

        // Sequence number - 12 bits (A local counter per machine that rolls over every 4096)
        // The sequence number is used to generate multiple ids per millisecond. This means we can generate
        // 4095 ids per ms and must then wait until the next ms before we can continue generating ids.
        private const int SequenceBits = 12;

        private static readonly int MaxNodeId = (int)(Mathf.Pow(2, MachineIdBits) - 1);
        private static readonly int MaxSequence = (int)(Mathf.Pow(2, SequenceBits) - 1);

        /// <summary>
        /// The name of the EditorPrefs that is used to store the machine id.
        /// </summary>
        public const string MachineIdPrefKey = "KeyGenerator-MachineId";

        private readonly long _customEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        private long _lastTimestamp = -1;
        private long _sequence;
        private int _machineId;

        /// <summary>
        /// The custom epoch used to generate the timestamp.
        /// </summary>
        public long CustomEpoch => _customEpoch;

        /// <summary>
        /// The Id of the current machine. By default, in the Editor, this value is generated
        /// from the machines network interface physical address however it can also be set to a user provided value.
        /// There is enough space for 1024 unique machines.
        /// Set value will be clamped in the range 1-1023.
        /// The value is not serialized into the asset but stored into the EditorPrefs(Editor only).
        /// </summary>
        public int MachineId
        {
            get
            {
                if (_machineId == 0)
                {
                    _machineId = GetMachineId();
                }
                return _machineId;
            }

            set
            {
                _machineId = Mathf.Clamp(value, 1, MaxNodeId);

#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetInt(MachineIdPrefKey, _machineId);
#endif
            }
        }

        /// <summary>
        /// Create a default instance which uses the current time as the <see cref="CustomEpoch"/> the machines
        /// physical address as <see cref="MachineId"/>.
        /// </summary>
        public SnowflakeIdGenerator()
        {
        }

        /// <summary>
        /// Creates an instance with a defined <see cref="CustomEpoch"/>.
        /// </summary>
        /// <param name="customEpoch">The custom epoch is used to calculate the timestamp by taking the
        /// current time and subtracting the <see cref="CustomEpoch"/>.
        /// The value is then stored in 41 bits giving it a maximum time of 69 years.</param>
        public SnowflakeIdGenerator(long customEpoch)
        {
            _customEpoch = customEpoch;
        }

        /// <summary>
        /// Returns the next Id using the current time, machine id and sequence number.
        /// </summary>
        /// <returns></returns>
        public long GetNextKey()
        {
            var currentTimestamp = GetTimestamp();

            Debug.Assert(currentTimestamp >= _lastTimestamp, "Invalid system clock. Current time is less than previous time.");

            // If we are generating another id in the same millisecond then we need to increment the sequence
            // or wait till the next millisecond if we have exhausted our sequences.
            if (currentTimestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & MaxSequence;
                if (_sequence == 0)
                {
                    // Sequence Exhausted, wait till next millisecond.
                    currentTimestamp = WaitNextMs(currentTimestamp);
                }
            }
            else
            {
                // reset sequence to start with zero for the next millisecond.
                _sequence = 0;
            }

            _lastTimestamp = currentTimestamp;

            long id = currentTimestamp << (MachineIdBits + SequenceBits);
            id |= (uint)MachineId << SequenceBits;
            id |= _sequence;
            return id;
        }

        private long GetTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _customEpoch;

        private static int GetMachineId()
        {
#if UNITY_EDITOR
            var id = UnityEditor.EditorPrefs.GetInt(MachineIdPrefKey, 0);
            if (id != 0)
            {
                return id;
            }

            foreach (var nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                {
                    var address = nic.GetPhysicalAddress().ToString();
                    return address.GetHashCode() & MaxNodeId;
                }
            }
#endif
            return Random.Range(0, MaxNodeId);
        }

        // Block and wait till next millisecond
        private long WaitNextMs(long currentTimestamp)
        {
            while (currentTimestamp == _lastTimestamp)
            {
                System.Threading.Thread.Sleep(1);
                currentTimestamp = GetTimestamp();
            }
            return currentTimestamp;
        }
    }
}
