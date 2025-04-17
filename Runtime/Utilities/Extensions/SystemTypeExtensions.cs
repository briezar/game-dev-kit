using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SystemTypeExtensions
{
    public static bool Implements<T>(this Type source)
    {
        return typeof(T).IsAssignableFrom(source);
    }

    public static bool Implements(this Type source, Type other)
    {
        return other.IsAssignableFrom(source);
    }
}