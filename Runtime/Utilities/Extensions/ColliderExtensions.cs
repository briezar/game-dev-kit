using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ColliderExtensions
{
    public static bool Contains(this Collider collider, Vector3 worldPoint)
    {
        if (collider is MeshCollider meshCollider && !meshCollider.convex)
        {
            Debug.LogWarning($"{collider.name} has a non-convex MeshCollider. Falling back to bounding box sampling.", collider);
            return collider.ClosestPointOnBounds(worldPoint) == worldPoint;
        }
        return collider.ClosestPoint(worldPoint) == worldPoint;
    }

    public static Vector3 GetRandomPoint(this Collider collider)
    {
        switch (collider)
        {
            case BoxCollider box:
                {
                    var localPoint = new Vector3(
                        Random.Range(-0.5f, 0.5f) * box.size.x,
                        Random.Range(-0.5f, 0.5f) * box.size.y,
                        Random.Range(-0.5f, 0.5f) * box.size.z
                    );
                    return box.transform.TransformPoint(box.center + localPoint);
                }
            case SphereCollider sphere:
                {
                    return sphere.transform.position + (Random.insideUnitSphere * sphere.radius);
                }
            case CapsuleCollider capsule:
                {
                    // 1. Calculate the internal line height (distance between center of caps)
                    float halfHeight = (capsule.height / 2f) - capsule.radius;
                    halfHeight = Mathf.Max(0, halfHeight); // Ensure it's not negative

                    // 2. Pick a random point on that central line (from cap to cap)
                    Vector3 randomLinePoint;
                    float randomZ = Random.Range(-halfHeight, halfHeight);

                    // 3. Pick a random direction in a sphere (the thickness)
                    Vector3 randomOffset = Random.insideUnitSphere * capsule.radius;

                    // 4. Align based on the capsule's direction (0=X, 1=Y, 2=Z)
                    if (capsule.direction == 0) { randomLinePoint = new Vector3(randomZ, 0, 0); }
                    else if (capsule.direction == 1) { randomLinePoint = new Vector3(0, randomZ, 0); }
                    else { randomLinePoint = new Vector3(0, 0, randomZ); }

                    // 5. Combine and transform to world space
                    return capsule.transform.TransformPoint(randomLinePoint + randomOffset + capsule.center);
                }
            default:
                return collider.GetRandomPointRejectionSampling();
        }
    }

    public static Vector3 GetRandomPointRejectionSampling(this Collider collider, int maxAttempts = 100)
    {
        var point = Vector3.zero;
        for (int attempts = 0; attempts < maxAttempts; attempts++)
        {
            point = collider.bounds.GetRandomPoint();
            if (collider.Contains(point)) { break; }

            if (attempts == maxAttempts - 1)
            {
                Debug.LogWarning($"Unable to find random point for {collider.name}, returning default", collider);
            }
        }

        return point;
    }

    public static bool Contains(this Collider2D collider, Vector2 worldPoint) => collider.ClosestPoint(worldPoint) == worldPoint;

    public static Vector2 GetRandomPoint(this Collider2D collider, int maxAttempts = 100)
    {
        switch (collider)
        {
            case BoxCollider2D box:
                {
                    var localPoint = new Vector2(
                        Random.Range(-0.5f, 0.5f) * box.size.x,
                        Random.Range(-0.5f, 0.5f) * box.size.y
                    );
                    return box.transform.TransformPoint(box.offset + localPoint);
                }
            case CircleCollider2D circle:
                {
                    var lossy = circle.transform.lossyScale;
                    var scale = Mathf.Max(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y));
                    return circle.transform.TransformPoint(circle.offset + circle.radius * scale * Random.insideUnitCircle);
                }
            case CapsuleCollider2D capsule:
                {
                    // approximate capsule by central line + radius
                    var size = capsule.size;
                    var vertical = capsule.direction == CapsuleDirection2D.Vertical;
                    var radius = vertical ? size.x * 0.5f : size.y * 0.5f;
                    var halfHeight = ((vertical ? size.y : size.x) / 2f) - radius;
                    halfHeight = Mathf.Max(0, halfHeight);

                    var randomLine = Random.Range(-halfHeight, halfHeight);
                    var linePoint = vertical ? new Vector2(0, randomLine) : new Vector2(randomLine, 0);
                    var offset = Random.insideUnitCircle * radius;
                    return capsule.transform.TransformPoint(capsule.offset + linePoint + offset);
                }
            default:
                return collider.GetRandomPointRejectionSampling(maxAttempts);
        }
    }

    public static Vector2 GetRandomPointRejectionSampling(this Collider2D collider, int maxAttempts = 100)
    {
        var point = Vector2.zero;
        var b = collider.bounds;
        for (int attempts = 0; attempts < maxAttempts; attempts++)
        {
            point = new Vector2(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y));
            if (collider.Contains(point)) { break; }

            if (attempts == maxAttempts - 1)
            {
                Debug.LogWarning($"Unable to find random point for {collider.name}, returning default", collider);
            }
        }

        return point;
    }

}
