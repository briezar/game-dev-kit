using System;
using UnityEngine;
using UnityEditor;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDate))]
    public class SerializableDateDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var timestampProp = property.FindPropertyRelative(SerializableDate.EditorProps.TimestampMs);
            var timestamp = timestampProp.longValue;
            var localDate = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).ToLocalTime();

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = 2f;

            var labelRect = new Rect(position.x, position.y, position.width, lineHeight);
            var fieldRect = new Rect(position.x, position.y + lineHeight + spacing, position.width - 50f, lineHeight);
            var buttonRect = new Rect(position.x + position.width - 50f, position.y + lineHeight + spacing, 50f, lineHeight);

            // Corrected label
            EditorGUI.LabelField(labelRect, label.text, $"Local: {localDate:yyyy-MM-dd HH:mm:ss}");

            // User input field
            var previousInput = timestamp.ToString();
            var newInput = EditorGUI.DelayedTextField(fieldRect, timestamp.ToString());

            // Only process and log if changed
            if (newInput != previousInput)
            {
                if (long.TryParse(newInput, out var parsedLong))
                {
                    if (parsedLong > 1_000_000_000_000L)
                    {
                        Debug.Log($"[SerializableDate] Parsed as Unix milliseconds: {parsedLong}");
                        timestampProp.longValue = parsedLong;
                    }
                    else if (parsedLong > 1_000_000_000L)
                    {
                        Debug.Log($"[SerializableDate] Parsed as Unix seconds: {parsedLong}");
                        timestampProp.longValue = parsedLong * 1000;
                    }
                }
                else if (DateTimeOffset.TryParse(newInput, out var parsedDate))
                {
                    Debug.Log($"[SerializableDate] Parsed as DateTimeOffset: {parsedDate}");
                    timestampProp.longValue = parsedDate.ToUniversalTime().ToUnixTimeMilliseconds();
                }
            }

            // "Now" button
            if (GUI.Button(buttonRect, "Now"))
            {
                timestampProp.longValue = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 2;
        }
    }
}