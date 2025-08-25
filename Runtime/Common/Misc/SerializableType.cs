using System;
using UnityEngine;

namespace GameDevKit
{
    public class TypeFilterAttribute : PropertyAttribute
    {
        public readonly bool AllowAbstract;
        public readonly bool AllowInterface;
        public readonly bool AllowGeneric;
        public readonly Type FilterType;

        public TypeFilterAttribute(Type filterType, bool allowAbstract = false, bool allowInterface = false, bool allowGeneric = false)
        {
            (FilterType, AllowAbstract, AllowInterface, AllowGeneric) = (filterType, allowAbstract, allowInterface, allowGeneric);
        }

        public bool IsValidType(Type type)
        {
            if (!AllowAbstract && type.IsAbstract) { return false; }
            if (!AllowInterface && type.IsInterface) { return false; }
            if (!AllowGeneric && type.IsGenericType) { return false; }

            return FilterType == null || type.Implements(FilterType);
        }

        public string GetFilterInfo()
        {
            return $"Valid types: {(FilterType != null ? FilterType : "Any")}, AllowAbstract: {AllowAbstract}, AllowInterface: {AllowInterface}, AllowGeneric: {AllowGeneric}";
        }
    }

    [Serializable]
    public struct SerializableType : IEquatable<SerializableType>, IEquatable<Type>
    {
        [SerializeField] private string _assemblyQualifiedName;

        private Type _type;
        public Type Type => _type ??= Type.GetType(_assemblyQualifiedName, true);

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