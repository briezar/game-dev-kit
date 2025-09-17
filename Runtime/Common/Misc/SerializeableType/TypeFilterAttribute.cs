using System;
using UnityEngine;

namespace GameDevKit
{
    public class TypeFilterAttribute : PropertyAttribute
    {
        public readonly bool AllowAbstract;
        public readonly bool AllowInterface;
        public readonly bool AllowGeneric;
        public readonly Type FilterType;

        public TypeFilterAttribute(Type filterType, bool allowAbstract = false, bool allowInterface = false, bool allowGeneric = false)
        {
            (FilterType, AllowAbstract, AllowInterface, AllowGeneric) = (filterType, allowAbstract, allowInterface, allowGeneric);
        }

        public bool IsValidType(Type type)
        {
            if (!AllowAbstract && type.IsAbstract) { return false; }
            if (!AllowInterface && type.IsInterface) { return false; }
            if (!AllowGeneric && type.IsGenericType) { return false; }

            return FilterType == null || type.Implements(FilterType);
        }

        public string GetFilterInfo()
        {
            return $"Valid types: {(FilterType != null ? FilterType : "Any")}, AllowAbstract: {AllowAbstract}, AllowInterface: {AllowInterface}, AllowGeneric: {AllowGeneric}";
        }
    }
}