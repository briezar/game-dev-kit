using System;
using UnityEngine;

public static class VectorExtensions
{
    public static int Count(this Vector2Int thisVector) => thisVector.x * thisVector.y;

    public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        => new(x ?? vector.x, y ?? vector.y, z ?? vector.z);
    public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
        => new(x ?? vector.x, y ?? vector.y);

    public static Vector3 Shift(this Vector3 vector, float x = 0, float y = 0, float z = 0) => vector + new Vector3(x, y, z);
    public static Vector2 Shift(this Vector2 vector, float x = 0, float y = 0) => vector + new Vector2(x, y);
}
