using UnityEngine;
using UnityEditor;

namespace GameDevKit.ObjectReferences.Editor
{
    public abstract class SingleLineObjectReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var objectProperty = property.FindPropertyRelative(GetObjectName());
            EditorGUI.PropertyField(position, objectProperty, label);
            EditorGUI.EndProperty();
        }

        protected abstract string GetObjectName();
    }

    [CustomPropertyDrawer(typeof(FolderReference), true)]
    public class FolderReferenceDrawer : SingleLineObjectReferenceDrawer
    {
        protected override string GetObjectName() => "_folderAsset";
    }

    [CustomPropertyDrawer(typeof(PathReference), true)]
    public class PathReferenceDrawer : SingleLineObjectReferenceDrawer
    {
        protected override string GetObjectName() => "_asset";
    }

}