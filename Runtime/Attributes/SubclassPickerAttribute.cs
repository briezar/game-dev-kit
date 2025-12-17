using UnityEngine;

namespace GameDevKit.Attributes
{
    /// <summary>
    /// Use in combination with <see cref="SerializeReference"/> to populate the field
    /// </summary>
    public class SubclassPickerAttribute : PropertyAttribute
    {
        public bool IgnoreUnityTypes = true;
    }
}