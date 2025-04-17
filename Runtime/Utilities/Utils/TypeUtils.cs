using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class TypeUtils
{
    public static Type[] GetDerivedTypesInCurrentDomain<T>() => AppDomain.CurrentDomain.GetDerivedTypes<T>();
    public static Type[] GetTypesWithInterfaceInCurrentDomain<T>() => AppDomain.CurrentDomain.GetTypesWithInterface<T>();

    public static Type[] GetDerivedTypes(this AppDomain appDomain, Type typeToFind)
    {
        var result = new List<Type>();
        var assemblies = appDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeToFind))
                    result.Add(type);
            }
        }
        return result.ToArray();
    }
    public static Type[] GetDerivedTypes<T>(this AppDomain appDomain) => GetDerivedTypes(appDomain, typeof(T));

    public static Type[] GetTypesWithInterface(this AppDomain appDomain, Type interfaceType)
    {
        var result = new List<Type>();
        var assemblies = appDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (interfaceType.IsAssignableFrom(type))
                    result.Add(type);
            }
        }
        return result.ToArray();
    }
    public static Type[] GetTypesWithInterface<T>(this AppDomain aAppDomain) => GetTypesWithInterface(aAppDomain, typeof(T));

    public static List<T> GetAllPublicConstantValues<T>(this Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(T))
            .Select(x => (T)x.GetRawConstantValue())
            .ToList();
    }
}