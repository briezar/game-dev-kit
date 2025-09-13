using UnityEditor;
using GameDevKit.ObjectReferences;

namespace GameDevKit.Editor.ObjectReferences
{
    [CustomPropertyDrawer(typeof(PathReference), true)]
    public class PathReferenceDrawer : SingleLineObjectReferenceDrawer
    {
        protected override string GetObjectName() => "_asset";
    }

}