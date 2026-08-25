using UnityEditor;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(SerializableShaderProperty), true)]
    public class SerializableShaderPropertyDrawer : SingleLineDrawer
    {
        protected override string GetObjectName() => SerializableShaderProperty.EditorProps.PropertyName;
    }

}