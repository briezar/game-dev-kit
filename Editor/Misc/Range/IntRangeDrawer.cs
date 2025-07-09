using UnityEngine;
using UnityEditor;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(IntRange))]
    public class IntRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label (and adjust our position to the content area)
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Calculate rects
            Rect minRect = new Rect(position.x, position.y, position.width / 2 - 8, position.height);
            Rect maxRect = new Rect(minRect.xMax + 8, position.y, position.width / 2, position.height);

            EditorGUIUtility.labelWidth = 40;
            EditorGUI.PropertyField(minRect, property.FindPropertyRelative("min"));
            EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("max"), new GUIContent("Max"));

            EditorGUI.EndProperty();
        }
    }
}