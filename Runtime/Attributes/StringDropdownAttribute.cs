using System;
using UnityEngine;

namespace GameDevKit.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class StringDropdownAttribute : PropertyAttribute
    {
        public string MethodName { get; }

        public StringDropdownAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}