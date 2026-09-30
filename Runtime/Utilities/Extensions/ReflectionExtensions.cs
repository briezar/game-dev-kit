using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class ReflectionExtensions
{
    public static bool Implements<T>(this Type source) => Implements(source, typeof(T));
    public static bool Implements(this Type source, Type other)
    {
        if (!other.IsGenericTypeDefinition) { return other.IsAssignableFrom(source); }
        if (other.IsInterface)
        {
            foreach (var interfaceType in source.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == other)
                {
                    return true;
                }
            }
            return false;
        }

        for (var current = source; current != null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == other)
            {
                return true;
            }
        }
        return false;
    }

    public static bool HasAttribute<TAttribute>(this object source, bool inherit = true) where TAttribute : Attribute => HasAttribute<TAttribute>(source.GetType(), inherit);
    public static bool HasAttribute<TAttribute>(this Type type, bool inherit = true) where TAttribute : Attribute => type.IsDefined(typeof(TAttribute), inherit);

    public static bool TryGetAttribute<TAttribute>(this object source, out TAttribute attribute, bool inherit = true) where TAttribute : Attribute
    {
        attribute = source.GetType().GetCustomAttribute<TAttribute>(inherit);
        return attribute != null;
    }

}