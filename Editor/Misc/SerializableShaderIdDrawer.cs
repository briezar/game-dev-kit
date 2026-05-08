using UnityEditor;
using GameDevKit.Editor.ObjectReferences;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(SerializableShaderId), true)]
    public class SerializableShaderIdDrawer : SingleLineObjectReferenceDrawer
    {
        protected override string GetObjectName() => "_propertyName";
    }

}