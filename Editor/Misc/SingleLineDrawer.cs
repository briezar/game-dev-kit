using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace GameDevKit.Editor
{
    /// <summary>
    /// Base class for drawing a single line object reference in the inspector. The derived class should specify the name of the object property to draw by implementing GetObjectName().
    /// </summary>
    public abstract class SingleLineDrawer : PropertyDrawer
    {
        private SerializedProperty GetObjectProperty(SerializedProperty property) => property.FindPropertyRelative(GetObjectName());

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var objectProperty = GetObjectProperty(property);
            EditorGUI.PropertyField(position, objectProperty, label);
            EditorGUI.EndProperty();
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new PropertyField(GetObjectProperty(property));
            var labelText = property.displayName;
            field.RegisterCallbackOnce<GeometryChangedEvent>(_ => field.Q<Label>().text = labelText);
            return field;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(GetObjectProperty(property), label, true);

        protected abstract string GetObjectName();
    }

}