using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    /// <summary>
    /// Represents a 2D coordinate
    /// </summary>
    [Serializable]
    public struct Coordinate2D : IEquatable<Coordinate2D>
    {
        public enum AdjacentType { Orthogonal, Diagonal, Any }

        public int x, y;

        public Coordinate2D(int x, int y) => (this.x, this.y) = (x, y);

        public static Coordinate2D zero => new(0, 0);
        public static Coordinate2D one => new(1, 1);

        public static Coordinate2D up => new(0, 1);
        public static Coordinate2D down => new(0, -1);
        public static Coordinate2D left => new(-1, 0);
        public static Coordinate2D right => new(1, 0);

        private static readonly Coordinate2D[] _cardinalDirections = new Coordinate2D[] { up, right, down, left };

        public static ReadOnlySpan<Coordinate2D> CardinalDirections => _cardinalDirections;

        public readonly bool IsAdjacentTo(Coordinate2D other, AdjacentType adjacentType) => adjacentType switch
        {
            AdjacentType.Orthogonal => Math.Abs(x - other.x) + Math.Abs(y - other.y) == 1,
            AdjacentType.Diagonal => Math.Abs(x - other.x) == 1 && Math.Abs(y - other.y) == 1,
            AdjacentType.Any => Math.Max(Math.Abs(x - other.x), Math.Abs(y - other.y)) == 1,
            _ => false,
        };

        public readonly bool Equals(Coordinate2D other) => x == other.x && y == other.y;
        public override readonly bool Equals(object obj) => obj is Coordinate2D other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(x, y);

        public override readonly string ToString() => $"({x}, {y})";

        public static bool operator ==(Coordinate2D left, Coordinate2D right) => left.Equals(right);
        public static bool operator !=(Coordinate2D left, Coordinate2D right) => !left.Equals(right);

        public static Coordinate2D operator +(Coordinate2D a, Coordinate2D b) => new(a.x + b.x, a.y + b.y);
        public static Coordinate2D operator -(Coordinate2D a, Coordinate2D b) => new(a.x - b.x, a.y - b.y);

        public static implicit operator Vector2Int(Coordinate2D c) => new(c.x, c.y);
        public static implicit operator Coordinate2D(Vector2Int v) => new(v.x, v.y);

        public static implicit operator Vector3Int(Coordinate2D c) => new(c.x, c.y, 0);
        public static implicit operator Coordinate2D(Vector3Int v) => new(v.x, v.y);
    }
}