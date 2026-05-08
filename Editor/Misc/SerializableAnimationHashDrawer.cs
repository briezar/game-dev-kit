using UnityEditor;
using GameDevKit.Editor.ObjectReferences;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(SerializableAnimationHash), true)]
    public class SerializableAnimationHashDrawer : SingleLineObjectReferenceDrawer
    {
        protected override string GetObjectName() => "_paramName";
    }

}