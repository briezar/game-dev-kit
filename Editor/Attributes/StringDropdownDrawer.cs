using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameDevKit.Attributes;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.Attributes
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
            object declaringObject = GetDeclaringObject(property);

            if (declaringObject == null)
            {
                Debug.LogWarning("Declaring object for dropdown not found.");
                return null;
            }

            Type targetType = declaringObject.GetType();
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

            return method.Invoke(declaringObject, null) as IEnumerable<string>;
        }

        private object GetDeclaringObject(SerializedProperty property)
        {
            object obj = property.serializedObject.targetObject;
            string[] path = property.propertyPath.Replace(".Array.data[", "[")
                                                 .Split('.');

            // We stop before the last element, which is the actual property itself (e.g., a string)
            for (int i = 0; i < path.Length - 1; i++)
            {
                string element = path[i];
                if (element.Contains("["))
                {
                    string fieldName = element.Substring(0, element.IndexOf("["));
                    int index = int.Parse(element.Substring(element.IndexOf("[") + 1, element.IndexOf("]") - element.IndexOf("[") - 1));
                    obj = GetFieldValue(obj, fieldName, index);
                }
                else
                {
                    obj = GetFieldValue(obj, element);
                }

                if (obj == null)
                    return null;
            }

            return obj;
        }

        private object GetFieldValue(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();
            FieldInfo f = null;

            while (type != null)
            {
                f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null) break;
                type = type.BaseType;
            }

            return f?.GetValue(source);
        }

        private object GetFieldValue(object source, string name, int index)
        {
            IEnumerable enumerable = GetFieldValue(source, name) as IEnumerable;
            if (enumerable == null) return null;

            IEnumerator enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext()) return null;
            }
            return enumerator.Current;
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