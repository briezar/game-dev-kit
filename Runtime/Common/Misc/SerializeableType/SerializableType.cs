using System;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public struct SerializableType : IEquatable<SerializableType>, IEquatable<Type>
    {
        [SerializeField] private string _assemblyQualifiedName;

        public readonly string AssemblyQualifiedName => _assemblyQualifiedName;

        private Type _type;
        public Type Type
        {
            get
            {
                if (_type == null)
                {
                    _type = Type.GetType(_assemblyQualifiedName);
                    if (_type == null)
                    {
                        Debug.LogWarning($"[SerializableType] Failed to resolve type from AssemblyQualifiedName '{_assemblyQualifiedName}'");
                        _assemblyQualifiedName = string.Empty;
                        return null;
                    }
                }
                return _type;
            }
        }

        public bool IsValid => !_assemblyQualifiedName.IsNullOrEmpty() && Type != null;

        public static class EditorProps
        {
            public const string AssemblyQualifiedName = nameof(_assemblyQualifiedName);
        }

        public SerializableType(Type type)
        {
            _assemblyQualifiedName = type.AssemblyQualifiedName;
            _type = type;
        }

        public static implicit operator Type(SerializableType serializableType) => serializableType.Type;
        public static implicit operator SerializableType(Type type) => new(type);

        public static bool operator ==(SerializableType left, SerializableType right) => left.Equals(right);
        public static bool operator !=(SerializableType left, SerializableType right) => !left.Equals(right);

        public static bool operator ==(SerializableType left, Type right) => left.Equals(right);
        public static bool operator !=(SerializableType left, Type right) => !left.Equals(right);

        public readonly bool Equals(SerializableType other) => string.Equals(_assemblyQualifiedName, other._assemblyQualifiedName, StringComparison.Ordinal);
        public readonly bool Equals(Type other) => string.Equals(_assemblyQualifiedName, other.AssemblyQualifiedName, StringComparison.Ordinal);

        public override readonly bool Equals(object obj) => obj switch
        {
            SerializableType serializableType => Equals(serializableType),
            Type type => Equals(type),
            _ => false,
        };

        public override readonly int GetHashCode() => _assemblyQualifiedName.GetHashCode();
        public override readonly string ToString() => _assemblyQualifiedName;
    }
}