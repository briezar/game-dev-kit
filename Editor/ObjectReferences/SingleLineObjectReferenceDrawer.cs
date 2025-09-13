using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace GameDevKit.Editor.ObjectReferences
{
    public abstract class SingleLineObjectReferenceDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, SerializedProperty> _cache = new();

        private SerializedProperty GetObjectProperty(SerializedProperty property)
        {
            var path = property.propertyPath;
            if (!_cache.TryGetValue(path, out var cached))
            {
                cached = property.FindPropertyRelative(GetObjectName());
                if (cached == null)
                {
                    Debug.LogError($"Could not find property '{GetObjectName()}' in '{property.propertyPath}'.");
                }
                _cache[path] = cached;
            }
            return cached;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var objectProperty = GetObjectProperty(property);
            EditorGUI.PropertyField(position, objectProperty, label);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(GetObjectProperty(property), label, true);

        protected abstract string GetObjectName();
    }

}