using System;
using System.Linq;
using GameDevKit.Identifiers;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor.Identifiers
{
    [CustomPropertyDrawer(typeof(UniGuid))]
    public class UniGuidDrawer : PropertyDrawer
    {
        private static readonly string[] GuidParts = { "a", "b", "c", "d" };

        private SerializedProperty[] _guidParts = new SerializedProperty[GuidParts.Length];

        private SerializedProperty[] GetGuidParts(SerializedProperty property)
        {
            for (int i = 0; i < GuidParts.Length; i++)
            {
                _guidParts[i] = property.FindPropertyRelative(GuidParts[i]);
            }

            return _guidParts;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            GetGuidParts(property);
            var stringGuid = BuildGuidString(_guidParts);

            if (_guidParts.Any(x => x == null || x.uintValue == 0))
            {
                var objName = property.serializedObject.targetObject.name;
                Debug.Log($"UniGuid of object {objName} is invalid ({stringGuid}). Auto-regenerated a new UniGuid.");
                property.boxedValue = UniGuid.NewGuid();
            }


            GUI.enabled = false;
            EditorGUI.TextField(position, label, stringGuid);
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
            menu.AddItem(new GUIContent("Regenerate GUID"), false, () => RegenerateGuid(property));
            menu.ShowAsContext();
        }

        void CopyGuid(SerializedProperty property)
        {
            if (_guidParts.Any(x => x == null || x.uintValue == 0)) return;

            string guid = BuildGuidString(_guidParts);
            EditorGUIUtility.systemCopyBuffer = guid;
            Debug.Log($"GUID copied to clipboard: {guid}");
            var a = _guidParts[0].uintValue;
            var b = _guidParts[1].uintValue;
            var c = _guidParts[2].uintValue;
            var d = _guidParts[3].uintValue;
            Debug.Log($"{a}-{b}-{c}-{d}");
        }

        void RegenerateGuid(SerializedProperty property)
        {
            const string warning = "Are you sure you want to regenerate the GUID?";
            if (!EditorUtility.DisplayDialog("Reset GUID", warning, "Yes", "No")) return;

            property.boxedValue = UniGuid.NewGuid();
            property.serializedObject.ApplyModifiedProperties();
            Debug.Log("GUID has been regenerated.");
        }

        private static string BuildGuidString(SerializedProperty[] guidParts)
        {
            var a = guidParts[0].uintValue;
            var b = guidParts[1].uintValue;
            var c = guidParts[2].uintValue;
            var d = guidParts[3].uintValue;
            return $"{a:X8}{b:X8}{c:X8}{d:X8}";
        }
    }
}