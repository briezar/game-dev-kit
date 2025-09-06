using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class DebugUtils
{
    [Conditional("UNITY_EDITOR")]
    public static void DrawCircle(Vector2 center, float radius, Color? color = null, float duration = 0.2f, int segments = 32)
    {
        float angleStep = 2 * Mathf.PI / segments;
        var drawColor = color ?? Color.green;

        Vector3 prevPoint = center + new Vector2(Mathf.Cos(0), Mathf.Sin(0)) * radius;
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep;
            Vector3 nextPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Debug.DrawLine(prevPoint, nextPoint, drawColor, duration);
            prevPoint = nextPoint;
        }
    }
}