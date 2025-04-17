using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class SystemObjectExtension
{
    public static string GetObjectInfo(this object obj, string separator = ", ")
    {
        var textList = new List<string>();
        var type = obj.GetType();
        var objName = $" [{type.Name.Colorize(Color.green)}] ";
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            textList.Add($"{field.Name.Colorize(Color.yellow)}: {field.GetValue(obj) ?? "null"}");
        }
        foreach (var property in properties)
        {
            textList.Add($"{property.Name.Colorize(ArtStyle.FontColor.paleYellow)}: {property.GetValue(obj) ?? "null"}");
        }
        return objName + textList.JoinToString(separator);
    }
}
