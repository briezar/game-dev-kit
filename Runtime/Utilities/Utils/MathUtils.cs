using UnityEngine;

public static class MathUtils
{
    /// <summary> <see cref="Mathf.InverseLerp"/> clamps between 0-1. This method can return values outside 0-1. </summary>
    public static float UnclampedInverseLerp(float a, float b, float value) => (value - a) / (b - a);

}