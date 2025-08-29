using System;
using GameDevKit.Identifiers;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.Identifiers
{
    [CustomPropertyDrawer(typeof(ShortGuid))]
    public class ShortGuidDrawer : PropertyDrawer
    {
        private SerializedProperty _stringGuidProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            _stringGuidProperty = property.FindPropertyRelative("_value");

            if (_stringGuidProperty.stringValue.IsNullOrEmpty())
            {
                var objName = property.serializedObject.targetObject.name;
                Debug.LogWarning($"ShortGuid of object {objName} is empty. Auto-regenerated a new guid.");
                property.boxedValue = ShortGuid.NewGuid();
            }

            GUI.enabled = false;
            EditorGUI.PropertyField(position, _stringGuidProperty, label, true);
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
            menu.AddItem(new GUIContent("Copy GUID"), false, () => CopyGuid(property));
            menu.AddItem(new GUIContent("Print Full GUID"), false, () => PrintFullGuid(property));
            // menu.AddItem(new GUIContent("Reset GUID"), false, () => ResetGuid(property));
            menu.AddItem(new GUIContent("Regenerate GUID"), false, () => RegenerateGuid(property));
            menu.ShowAsContext();
        }

        void CopyGuid(SerializedProperty property)
        {
            var stringGuid = _stringGuidProperty.stringValue;
            EditorGUIUtility.systemCopyBuffer = stringGuid;
            Debug.Log($"GUID copied to clipboard: {stringGuid}");
        }

        void PrintFullGuid(SerializedProperty property)
        {
            var shortGuid = _stringGuidProperty.stringValue;
            var fullGuid = ShortGuid.Decode(shortGuid);
            Debug.Log($"{shortGuid} | {fullGuid}");
        }

        // void ResetGuid(SerializedProperty property)
        // {
        //     const string warning = "Are you sure you want to reset the GUID?";
        //     if (!EditorUtility.DisplayDialog("Reset GUID", warning, "Yes", "No")) return;

        //     foreach (var part in GetGuidParts(property))
        //     {
        //         part.uintValue = 0;
        //     }

        //     property.serializedObject.ApplyModifiedProperties();
        //     Debug.Log("GUID has been reset.");
        // }

        void RegenerateGuid(SerializedProperty property)
        {
            const string warning = "Are you sure you want to regenerate the GUID?";
            if (!EditorUtility.DisplayDialog("Reset GUID", warning, "Yes", "No")) return;

            property.boxedValue = ShortGuid.NewGuid();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}