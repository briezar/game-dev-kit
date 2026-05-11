using System;
using GameDevKit.NewtonsoftJson;
using Newtonsoft.Json;

namespace GameDevKit
{
    public interface IObjectConverter
    {
        object Convert(object data);

        public static Func<JsonSerializer> DefaultSerializer = () => JsonSerializer.Create(new() { ContractResolver = new ExcludeGetOnlyPropertiesResolver() });
    }
}