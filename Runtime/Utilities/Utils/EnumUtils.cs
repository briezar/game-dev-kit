using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumUtils
{
    public static T[] GetEnums<T>() where T : Enum
    {
        var array = (T[])Enum.GetValues(typeof(T));
        return array;
    }

    public static T GetRandom<T>(T[] enumValues = null) where T : Enum
    {
        enumValues ??= GetEnums<T>();
        var randomIndex = UnityEngine.Random.Range(0, enumValues.Length);
        return enumValues[randomIndex];
    }

}