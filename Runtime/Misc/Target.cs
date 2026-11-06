using System;
using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public struct Target : IEquatable<Target>
    {
        [DisableField(nameof(_usePosition))]
        [SerializeField] private Transform _transform;

        [EnableField(nameof(_usePosition))]
        [SerializeField] private Vector3 _position;
        [SerializeField] private bool _usePosition;

        public static readonly Target NoTarget = new(null, null);

        public readonly bool HasTarget => _usePosition || _transform != null;
        public readonly Transform Transform => _transform;
        public readonly Vector3 Position => _usePosition ? _position : _transform != null ? _transform.position : default;

        public Target(GameObject gameObject) : this(gameObject != null ? gameObject.transform : null, null) { }
        public Target(Component component) : this(component != null ? component.transform : null, null) { }
        public Target(Vector3 position) : this(null, position) { }

        private Target(Transform transform, Vector3? position)
        {
            _transform = transform;
            _position = position ?? default;
            _usePosition = position.HasValue;
        }

        public readonly bool Equals(Target other) => _usePosition == other._usePosition && (_usePosition ? _position == other._position : _transform == other._transform);
        public override readonly bool Equals(object obj) => obj is Target other && Equals(other);
        public override readonly int GetHashCode() => _usePosition ? HashCode.Combine(true, _position) : HashCode.Combine(false, _transform != null ? _transform : null);

        public static bool operator ==(Target left, Target right) => left.Equals(right);
        public static bool operator !=(Target left, Target right) => !left.Equals(right);

        public static implicit operator Target(GameObject gameObject) => new(gameObject);
        public static implicit operator Target(Component component) => new(component);
        public static implicit operator Target(Vector3 position) => new(position);
    }
}