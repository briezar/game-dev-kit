using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ValueTypeExtension
{
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
        {
            value = min;
        }
        else if (value.CompareTo(max) > 0)
        {
            value = max;
        }
        return value;
    }

    public static T ClampMin<T>(this T value, T min) where T : IComparable<T>
    {
        return Clamp(value, min, value);
    }

    public static T ClampMax<T>(this T value, T max) where T : IComparable<T>
    {
        return Clamp(value, value, max);
    }

    public static int ClampCollection<T>(this int value, IEnumerable<T> enumerable)
    {
        if (enumerable.IsNullOrEmpty())
        {
            Debug.LogWarning("Invalid collection!");
            return 0;
        }
        return Mathf.Clamp(value, 0, enumerable.Count() - 1);
    }

    /// <summary> Returns true if a is approximate equal to b within an Epsilon (tolerance) </summary>
    /// <param name="tolerance"> 0.01f is large enough to compare 0.33f and 1f/3 </param>
    public static bool ApproximatelyEquals(this float a, float b, float tolerance = 0.01f) => Mathf.Abs(a - b) < tolerance;

    public static float Round(this float value, int decimalPlace)
    {
        // 0.1223 , 2 decimal place
        // 12.23
        // 12
        // 0.12
        var multiply = Mathf.Pow(10, decimalPlace);
        var roundedValue = Mathf.Round(value * multiply);
        return roundedValue / multiply;
    }

    public static Color With(this Color color, float? r = null, float? g = null, float? b = null, float? a = null)
        => new(r ?? color.r, g ?? color.g, b ?? color.b, a ?? color.a);

    public static Vector3 GetRandomPosition(this Rect rect)
    {
        var x = Random.Range(rect.xMin, rect.xMax);
        var y = Random.Range(rect.yMin, rect.yMax);
        return new Vector3(x, y);
    }

    public static Rect ScaleFromCenter(this Rect rect, float ratio) => ScaleFromCenter(rect, ratio, ratio);
    public static Rect ScaleFromCenter(this Rect rect, float widthRatio, float heightRatio)
    {
        var width = rect.width * widthRatio;
        var height = rect.height * heightRatio;
        return new Rect(rect.center.x - width / 2, rect.center.y - height / 2, width, height);
    }

}
