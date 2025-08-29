using System;
using GameDevKit.Identifiers;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.Identifiers
{
    [CustomPropertyDrawer(typeof(ShortId))]
    public class ShortIdDrawer : PropertyDrawer
    {
        private ShortId _shortId;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            _shortId = (ShortId)property.boxedValue;

            if (_shortId.value == 0)
            {
                var objName = property.serializedObject.targetObject.name;
                Debug.LogWarning($"ShortId of object {objName} is empty. Auto-generated a new Id.");
                property.boxedValue = ShortId.NewId();
            }

            GUI.enabled = false;
            EditorGUI.TextField(position, label, _shortId.ToString());
            // EditorGUI.PropertyField(position, _idProperty, label, true);
            GUI.enabled = true;

            bool hasClicked = Event.current.type == EventType.MouseUp && Event.current.button == 1;
            if (hasClicked && position.Contains(Event.current.mousePosition))
            {
                ShowContextMenu(property);
                Event.current.Use();
            }

            EditorGUI.EndProperty();
        }

        void ShowContextMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy ID"), false, () => CopyId(property));
            menu.AddItem(new GUIContent("Print Internal Value"), false, () => PrintInternalValue(property));
            menu.AddItem(new GUIContent("Regenerate ID"), false, () => RegenerateId(property));
            menu.ShowAsContext();
        }

        void CopyId(SerializedProperty property)
        {
            EditorGUIUtility.systemCopyBuffer = $"{_shortId}";
            Debug.Log($"GUID copied to clipboard: {_shortId}");
        }

        void PrintInternalValue(SerializedProperty property)
        {
            Debug.Log($"Value of {_shortId}: {_shortId.value}");
        }

        void RegenerateId(SerializedProperty property)
        {
            const string warning = "Are you sure you want to regenerate the ID?";
            if (!EditorUtility.DisplayDialog("Reset GUID", warning, "Yes", "No")) return;

            // Supports multi-object editing
            var selectedObjects = property.serializedObject.targetObjects;
            foreach (var obj in selectedObjects)
            {
                var selectedSerializedObj = new SerializedObject(obj);
                var selectedObjProperty = selectedSerializedObj.FindProperty(property.propertyPath);

                selectedObjProperty.boxedValue = ShortId.NewId();
                selectedObjProperty.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}