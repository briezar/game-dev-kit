using Newtonsoft.Json;
using UnityEngine;

namespace GameDevKit.NewtonsoftJson
{
    public static class JsonSerializerSettingsPresets
    {
        public static JsonSerializerSettings Minimized => new()
        {
            ContractResolver = new ExcludeGetOnlyPropertiesResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static JsonSerializerSettings Explicit => new()
        {
            ContractResolver = new ExcludeGetOnlyPropertiesResolver(),
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Populate,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public static JsonSerializerSettings WithPrettyPrint(this JsonSerializerSettings serializerSettings)
        {
            serializerSettings.Formatting = Formatting.Indented;
            return serializerSettings;
        }
    }
}