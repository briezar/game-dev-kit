using System;
using UnityEngine;

[Flags]
public enum AdjacentDirection
{
    None = 0,
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3
}

public static class VectorExtension
{
    public static int Count(this Vector2Int thisVector)
    {
        return thisVector.x * thisVector.y;
    }

    public static bool IsAdjacent(this Vector2Int thisVector, Vector2Int otherVector, AdjacentDirection direction)
    {
        var xDiff = thisVector.x - otherVector.x;
        var yDiff = thisVector.y - otherVector.y;

        var up = false;
        var down = false;
        var left = false;
        var right = false;

        if (direction.HasFlag(AdjacentDirection.Up))
        {
            up = xDiff == 0 && yDiff == 1;
        }

        if (direction.HasFlag(AdjacentDirection.Down))
        {
            down = xDiff == 0 && yDiff == -1;
        }

        if (direction.HasFlag(AdjacentDirection.Left))
        {
            left = xDiff == -1 && yDiff == 0;
        }

        if (direction.HasFlag(AdjacentDirection.Right))
        {
            right = xDiff == 1 && yDiff == 0;
        }

        return up || down || left || right;
    }

    public static bool IsAdjacent(this Vector2Int thisVector, Vector2Int otherVector)
    {
        var dx = Mathf.Abs(thisVector.x - otherVector.x);
        var dy = Mathf.Abs(thisVector.y - otherVector.y);
        return dx + dy == 1;
    }

    public static bool IsAdjacent(this Vector2Int thisVector, Vector2Int otherVector, out AdjacentDirection? direction)
    {
        direction = null;
        var isAdjacent = IsAdjacent(thisVector, otherVector);
        if (!isAdjacent) { return false; }

        if (otherVector.x < thisVector.x)
        {
            direction = AdjacentDirection.Left;
        }
        else if (otherVector.x > thisVector.x)
        {
            direction = AdjacentDirection.Right;
        }
        else
        {
            direction = otherVector.y < thisVector.y ? AdjacentDirection.Down : AdjacentDirection.Up;
        }

        return isAdjacent;
    }

    public static float GetPathDistance(this Vector3[] path)
    {
        if (path.IsNullOrEmpty() || path.Length < 2) { return 0; }

        var totalDistance = 0f;
        for (int i = 1; i < path.Length; i++)
        {
            var lastPoint = path[i - 1];
            var point = path[i];
            totalDistance += Vector3.Distance(lastPoint, point);
        }
        return totalDistance;
    }

    public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        => new(x ?? vector.x, y ?? vector.y, z ?? vector.z);
    public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
        => new(x ?? vector.x, y ?? vector.y);

    public static Vector3 Shift(this Vector3 vector, float x = 0, float y = 0, float z = 0) => vector + new Vector3(x, y, z);
    public static Vector2 Shift(this Vector2 vector, float x = 0, float y = 0) => vector + new Vector2(x, y);
}
