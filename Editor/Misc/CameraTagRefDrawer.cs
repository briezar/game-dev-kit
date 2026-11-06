using UnityEditor;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(CameraTagRef))]
    public class CameraTagRefDrawer : SingleLineDrawer
    {
        protected override string GetObjectName() => "_tag";
    }
}