using Newtonsoft.Json;
using UnityEngine;

public static class JsonExtensions
{
    public static T DeserializeNewtonsoft<T>(this object obj)
    {
        if (obj == null) { return default; }
        if (obj.GetType() == typeof(T)) { return (T)obj; }
        return JsonConvert.DeserializeObject<T>(obj.ToString());
    }

}