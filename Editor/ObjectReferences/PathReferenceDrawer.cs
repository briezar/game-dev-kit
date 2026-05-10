using UnityEditor;
using GameDevKit.ObjectReferences;

namespace GameDevKit.Editor.ObjectReferences
{
    [CustomPropertyDrawer(typeof(PathReference), true)]
    public class PathReferenceDrawer : SingleLineDrawer
    {
        protected override string GetObjectName() => "_asset";
    }

}