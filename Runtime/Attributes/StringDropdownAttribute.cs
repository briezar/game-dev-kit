using System;
using UnityEngine;

namespace GameDevKit.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class StringDropdownAttribute : PropertyAttribute
    {
        public string MethodName { get; }

        /// <summary> Creates a dropdown for string fields in the inspector. </summary>
        /// <param name="methodName"> The name of the method that returns a string array for the dropdown options. </param>
        public StringDropdownAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}