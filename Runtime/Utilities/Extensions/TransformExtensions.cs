using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class TransformExtensions
{
    public static int DestroyAllChildren(this Transform parent, Func<Transform, bool> condition = null)
    {
        var destroyCount = 0;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (condition == null || condition(child))
            {
                child.DestroyGameObject();
                destroyCount++;
            }
        }

        return destroyCount;
    }

    public static int DestroyAllChildrenImmediate(this Transform parent, Func<Transform, bool> condition = null)
    {
        var destroyCount = 0;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (condition == null || condition(child))
            {
                child.DestroyGameObjectImmediate();
                destroyCount++;
            }
        }
        return destroyCount;
    }

    public static IEnumerable<Transform> EnumerateChildren(this Transform parent, Func<Transform, bool> condition = null)
    {
        var count = parent.childCount;
        for (int i = 0; i < count; i++)
        {
            var child = parent.GetChild(i);
            if (condition == null || condition(child)) { yield return child; }
        }
    }

    public static IEnumerable<Transform> ReverseEnumerateChildren(this Transform parent, Func<Transform, bool> condition = null)
    {
        var count = parent.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (condition == null || condition(child)) { yield return child; }
        }
    }

    public static int GetChildCount(this Transform parent, Func<Transform, bool> condition)
    {
        var count = 0;
        var childCount = parent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var child = parent.GetChild(i);
            if (condition == null || condition(child)) { count++; }
        }
        return count;
    }

}