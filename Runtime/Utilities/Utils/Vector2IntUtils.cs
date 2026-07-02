using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace GameDevKit
{
    public enum AdjacentType { Orthogonal, Diagonal, Any }
    public static class Vector2IntUtils
    {
        public static ReadOnlyCollection<Vector2Int> CardinalDirections = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left }.AsReadOnly();

        public static bool IsAdjacent(Vector2Int a, Vector2Int b, AdjacentType adjacentType) => adjacentType switch
        {
            AdjacentType.Orthogonal => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) == 1,
            AdjacentType.Diagonal => Math.Abs(a.x - b.x) == 1 && Math.Abs(a.y - b.y) == 1,
            AdjacentType.Any => Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y)) == 1,
            _ => false,
        };
    }

    public static class Vector2IntExtensions
    {
        public static bool IsAdjacentTo(this Vector2Int a, Vector2Int b, AdjacentType adjacentType) => Vector2IntUtils.IsAdjacent(a, b, adjacentType);
    }
}