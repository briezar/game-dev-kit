using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class JsonExtensions
{
    public static T DeserializeNewtonsoft<T>(this object obj) => DeserializeNewtonsoft(obj, typeof(T)) is T result ? result : default;
    public static object DeserializeNewtonsoft(this object obj, Type type)
    {
        if (obj == null) { return default; }
        if (obj.GetType() == type) { return obj; }
        if (obj is JObject jObject) { return jObject.ToObject(type); }
        Debug.Log($"Deserializing {obj.GetType()} to {type}");
        return JsonConvert.DeserializeObject(obj.ToString(), type);
    }

    public static string ToJsonNewtonsoft(this object obj, Formatting formatting = Formatting.Indented)
    {
        if (obj == null) { return string.Empty; }
        return JsonConvert.SerializeObject(obj, formatting);
    }

    public static string ToJsonUnity(this object obj, bool prettyPrint = true)
    {
        if (obj == null) { return string.Empty; }
        return JsonUtility.ToJson(obj, prettyPrint);
    }

}