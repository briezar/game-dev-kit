using System;
using UnityEngine;

namespace GameDevKit.Attributes
{
    /// <summary>
    /// Use in combination with <see cref="SerializeReference"/> to populate the field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SubclassPickerAttribute : PropertyAttribute
    {
        public bool IgnoreUnityTypes = true;
    }
}