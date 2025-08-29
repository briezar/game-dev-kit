using UnityEngine;
using UnityEditor;
using GameDevKit.Attributes;

namespace GameDevKit.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(PrefabOnlyAttribute))]
    public class PrefabOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, fieldInfo.FieldType, false);
        }
    }
}