using UnityEditor;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(SerializableAnimationHash), true)]
    public class SerializableAnimationHashDrawer : SingleLineDrawer
    {
        protected override string GetObjectName() => SerializableAnimationHash.EditorProps.ParamName;
    }

}