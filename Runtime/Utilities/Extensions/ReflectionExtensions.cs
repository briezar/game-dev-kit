using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class ReflectionExtensions
{
    public static bool Implements<T>(this Type source)
    {
        return typeof(T).IsAssignableFrom(source);
    }

    public static bool Implements(this Type source, Type other)
    {
        return other.IsAssignableFrom(source);
    }

    public static bool HasAttribute<TAttribute>(this object source, bool inherit = true) where TAttribute : Attribute
    {
        return source.GetType().IsDefined(typeof(TAttribute), inherit);
    }

    public static bool TryGetAttribute<TAttribute>(this object source, out TAttribute attribute, bool inherit = true) where TAttribute : Attribute
    {
        attribute = source.GetType().GetCustomAttribute<TAttribute>(inherit);
        return attribute != null;
    }

}