using UnityEditor;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(SerializableShaderId), true)]
    public class SerializableShaderIdDrawer : SingleLineDrawer
    {
        protected override string GetObjectName() => "_propertyName";
    }

}