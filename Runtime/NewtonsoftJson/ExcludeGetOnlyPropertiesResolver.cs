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
            if (typeof(Delegate).IsAssignableFrom(property.PropertyType)) { return null; }
            if (member is PropertyInfo propertyInfo)
            {
                var hasSetter = propertyInfo.GetSetMethod(true) != null;
                if (!hasSetter)
                {
                    return null; // This catches { get; } and => 'expression bodied' properties
                }
            }
            return property;
        }
    }
}