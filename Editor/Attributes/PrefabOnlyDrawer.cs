using UnityEngine;
using UnityEditor;

namespace GameDevKit.Attributes.Editor
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