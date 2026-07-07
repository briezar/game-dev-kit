using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class YieldCollection
{
    public const float Epsilon = 0.0001f;
    private static readonly WaitForEndOfFrame _waitForEndOfFrame = new();
    private static readonly WaitForFixedUpdate _waitForFixedUpdate = new();

    // Adapted from https://forum.unity.com/threads/c-coroutine-waitforseconds-garbage-collection-tip.224878/#post-2436633
    private class FloatComparer : IEqualityComparer<float>
    {
        bool IEqualityComparer<float>.Equals(float x, float y)
        {
            return Mathf.Abs(x - y) <= Epsilon;
        }
        int IEqualityComparer<float>.GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }

    private static Dictionary<float, WaitForSeconds> _yieldInstructionDict = new(100, new FloatComparer());

    public static WaitForSeconds WaitForSeconds(float duration)
    {
        if (duration < (1f / Application.targetFrameRate)) { return null; }

        if (!_yieldInstructionDict.TryGetValue(duration, out var waitYield))
        {
            _yieldInstructionDict.Add(duration, waitYield = new WaitForSeconds(duration));
            // Debug.LogWarning($"New yield added: {duration:F5}, Count: {_yieldInstructionDict.Count}");
        }

        return waitYield;
    }

    public static WaitForSecondsRealtime WaitForSecondsRealtime(float duration)
    {
        if (duration < (1f / Application.targetFrameRate)) { return null; }
        var waitYield = new WaitForSecondsRealtime(duration);
        return waitYield;
    }

    public static WaitForEndOfFrame WaitForEndOfFrame() => _waitForEndOfFrame;
    public static WaitForFixedUpdate WaitForFixedUpdate() => _waitForFixedUpdate;

    public static IEnumerator WaitUntil(Func<bool> condition)
    {
        if (condition == null || condition()) { yield break; }
        while (!condition())
        {
            yield return null;
        }
    }

    public static IEnumerator WaitForFrames(int frameCount)
    {
        while (frameCount > 0)
        {
            frameCount--;
            yield return null;
        }
    }

    public static IEnumerator WaitUntilStableFPS(float threshold = 0.8f)
    {
        while ((1 / Time.smoothDeltaTime) < (Application.targetFrameRate * threshold))
        {
            yield return null;
        }
    }

    public static IEnumerator WaitForAnyKeyDown()
    {
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
    }

    public static IEnumerator WaitForAll(params YieldInstruction[] coroutines) => WaitForAll(coroutines.AsEnumerable());
    public static IEnumerator WaitForAll(IEnumerable<YieldInstruction> coroutines)
    {
        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }
    }
    public static IEnumerator WaitForAll(MonoBehaviour runner, params IEnumerator[] routines) => WaitForAll(runner, routines.AsEnumerable());
    public static IEnumerator WaitForAll(MonoBehaviour runner, IEnumerable<IEnumerator> routines)
    {
        return WaitForAll(routines.Where(routine => routine != null).Select(routine => runner.StartCoroutine(routine)));
    }

    public static IEnumerator WaitForAny(params YieldInstruction[] coroutines) => WaitForAny(coroutines.AsEnumerable());
    public static IEnumerator WaitForAny(IEnumerable<YieldInstruction> coroutines)
    {
        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
            break;
        }
    }
    public static IEnumerator WaitForAny(MonoBehaviour runner, params IEnumerator[] routines) => WaitForAny(runner, routines.AsEnumerable());
    public static IEnumerator WaitForAny(MonoBehaviour runner, IEnumerable<IEnumerator> routines)
    {
        return WaitForAny(routines.Where(routine => routine != null).Select(routine => runner.StartCoroutine(routine)));
    }

}

public static class YieldExtension
{
    public static bool ShouldYield(this IEnumerator enumerator)
    {
        return enumerator != null;
    }

    public static bool IsNull(this IEnumerator enumerator)
    {
        return enumerator == null;
    }

    public static IEnumerator AsIEnumerator(this YieldInstruction yieldInstruction)
    {
        yield return yieldInstruction;
    }
}
