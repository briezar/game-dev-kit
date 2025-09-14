using System;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonExtensions
{
    public static string ToJson(this object obj, Formatting formatting = Formatting.None)
    {
        if (obj == null) { return string.Empty; }
        return JsonConvert.SerializeObject(obj, formatting);
    }

    public static string ToPrettyJson(this object obj) => ToJson(obj, Formatting.Indented);

    public static string ToJsonUnity(this object obj, bool prettyPrint = true)
    {
        if (obj == null) { return string.Empty; }
        return JsonUtility.ToJson(obj, prettyPrint);
    }

}