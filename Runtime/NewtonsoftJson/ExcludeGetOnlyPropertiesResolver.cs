using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GameDevKit.NewtonsoftJson
{
    public class ExcludeGetOnlyPropertiesResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.HasMemberAttribute) { return property; }
            if (!property.Writable) { return null; }
            if (typeof(Delegate).IsAssignableFrom(property.PropertyType)) { return null; }
            return property;
        }
    }
}