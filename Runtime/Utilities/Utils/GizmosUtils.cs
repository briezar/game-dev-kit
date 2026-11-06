using System;
using UnityEngine;

public static class GizmosUtils
{
    public static void DrawCircle(Vector3 position, float radius, Vector3? up = null)
    {
        var upDirection = up ?? Vector3.up;
        var rotation = Quaternion.FromToRotation(Vector3.up, upDirection);
        using var scope = new MatrixScope(Matrix4x4.TRS(position, rotation, new Vector3(1, 0, 1)));
        Gizmos.DrawSphere(Vector3.zero, radius);
    }

    public static void DrawWireCircle(Vector3 position, float radius, Vector3? up = null)
    {
        var upDirection = up ?? Vector3.up;
        var rotation = Quaternion.FromToRotation(Vector3.up, upDirection);
        using var scope = new MatrixScope(Matrix4x4.TRS(position, rotation, new Vector3(1, 0, 1)));
        Gizmos.DrawWireSphere(Vector3.zero, radius);
    }

    public readonly struct MatrixScope : IDisposable
    {
        private readonly Matrix4x4 oldMatrix;

        public MatrixScope(Matrix4x4 newMatrix)
        {
            oldMatrix = Gizmos.matrix;
            Gizmos.matrix = newMatrix;
        }

        public void Dispose() => Gizmos.matrix = oldMatrix;
    }
}
