using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace GameDevKit.Attributes
{
    /// <summary>
    /// Use in combination with <see cref="SerializeReference"/> to populate the field
    /// </summary>
    public class SubclassPickerAttribute : PropertyAttribute
    {
        public SubclassPickerAttribute()
        {

        }
    }

#if UNITY_EDITOR
#endif
}