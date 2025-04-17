using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(StringDropdownAttribute))]
    public class StringDropdownDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String &&
                property.propertyType != SerializedPropertyType.Generic)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            StringDropdownAttribute dropdownAttribute = (StringDropdownAttribute)attribute;
            IEnumerable<string> options = GetDropdownValues(property, dropdownAttribute.MethodName);

            if (options == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            if (property.propertyType == SerializedPropertyType.String)
            {
                DrawSingleStringDropdown(position, property, label, options.ToArray());
            }
            else if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
            {
                DrawStringArrayDropdown(position, property, label, options.ToArray());
            }
        }

        private IEnumerable<string> GetDropdownValues(SerializedProperty property, string methodName)
        {
            object targetObject = property.serializedObject.targetObject;
            Type targetType = targetObject.GetType();
            MethodInfo method = targetType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (method == null)
            {
                Debug.LogWarning($"Method '{methodName}' not found on {targetType}.");
                return null;
            }

            if (!typeof(IEnumerable<string>).IsAssignableFrom(method.ReturnType))
            {
                Debug.LogWarning($"Method '{methodName}' must return IEnumerable<string>.");
                return null;
            }

            return method.Invoke(targetObject, null) as IEnumerable<string>;
        }

        private void DrawSingleStringDropdown(Rect position, SerializedProperty property, GUIContent label, string[] options)
        {
            int selectedIndex = Array.IndexOf(options, property.stringValue);
            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, options);

            if (selectedIndex >= 0)
            {
                property.stringValue = options[selectedIndex];
            }
        }

        private void DrawStringArrayDropdown(Rect position, SerializedProperty property, GUIContent label, string[] options)
        {
            EditorGUI.LabelField(position, label);
            position.y += EditorGUIUtility.singleLineHeight;

            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                int selectedIndex = Array.IndexOf(options, element.stringValue);
                selectedIndex = EditorGUI.Popup(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), $"Element {i}", selectedIndex, options);

                if (selectedIndex >= 0)
                {
                    element.stringValue = options[selectedIndex];
                }

                position.y += EditorGUIUtility.singleLineHeight;
            }
        }
    }
}