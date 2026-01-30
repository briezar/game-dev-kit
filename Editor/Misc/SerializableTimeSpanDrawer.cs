using System;
using UnityEngine;
using UnityEditor;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(SerializableTimeSpan))]
    public class SerializableTimeSpanDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var durationProp = property.FindPropertyRelative(SerializableTimeSpan.EditorProps.DurationMs);
            var durationMs = durationProp.longValue;
            var timeSpan = TimeSpan.FromMilliseconds(durationMs);

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = 2f;

            var labelRect = new Rect(position.x, position.y, position.width, lineHeight);
            var fieldRect = new Rect(position.x, position.y + lineHeight + spacing, position.width - 50f, lineHeight);
            var buttonRect = new Rect(position.x + position.width - 50f, position.y + lineHeight + spacing, 50f, lineHeight);

            // Display formatted duration in label
            var formattedDuration = FormatTimeSpan(timeSpan);
            var label1Content = new GUIContent(label.text, "Accepts parsable formats like 5s, 5m, 5h, 5d, or combinations like '2h 30m', '1d 12h 30m 15s'");
            var label2Content = new GUIContent(formattedDuration);
            EditorGUI.LabelField(labelRect, label1Content, label2Content);

            // User input field (milliseconds)
            var previousInput = durationMs.ToString();
            var newInput = EditorGUI.DelayedTextField(fieldRect, durationMs.ToString());

            // Only process and log if changed
            if (newInput != previousInput)
            {
                if (long.TryParse(newInput, out var parsedLong))
                {
                    Debug.Log($"[SerializableTimeSpan] Parsed as milliseconds: {parsedLong}");
                    durationProp.longValue = parsedLong;
                }
                else if (TryParseTimeSpanString(newInput, out var parsedTimeSpan))
                {
                    Debug.Log($"[SerializableTimeSpan] Parsed as TimeSpan: {parsedTimeSpan}");
                    durationProp.longValue = (long)parsedTimeSpan.TotalMilliseconds;
                }
            }

            // "Clear" button
            if (GUI.Button(buttonRect, "Clear"))
            {
                durationProp.longValue = 0;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 2;
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            var totalSeconds = timeSpan.TotalSeconds;
            var totalMinutes = timeSpan.TotalMinutes;
            var totalHours = timeSpan.TotalHours;
            var totalDays = timeSpan.TotalDays;

            if (totalMinutes < 1)
            {
                return $"{totalSeconds:0.##}s";
            }
            else if (totalHours < 1)
            {
                return $"{(int)totalMinutes}m {timeSpan.Seconds}s";
            }
            else if (totalDays < 1)
            {
                return $"{(int)totalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else
            {
                return $"{(int)totalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
        }

        private static bool TryParseTimeSpanString(string input, out TimeSpan result)
        {
            // Try standard TimeSpan.TryParse first
            if (TimeSpan.TryParse(input, out result))
            {
                return true;
            }

            // Try parsing custom formats like "1h 30m", "2d 3h", etc.
            try
            {
                var parts = input.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var totalMs = 0L;

                foreach (var part in parts)
                {
                    if (part.EndsWith("d") && double.TryParse(part.TrimEnd('d'), out var days))
                    {
                        totalMs += (long)(days * 86400000);
                    }
                    else if (part.EndsWith("h") && double.TryParse(part.TrimEnd('h'), out var hours))
                    {
                        totalMs += (long)(hours * 3600000);
                    }
                    else if (part.EndsWith("m") && double.TryParse(part.TrimEnd('m'), out var minutes))
                    {
                        totalMs += (long)(minutes * 60000);
                    }
                    else if (part.EndsWith("s") && double.TryParse(part.TrimEnd('s'), out var seconds))
                    {
                        totalMs += (long)(seconds * 1000);
                    }
                    else if (part.EndsWith("ms") && double.TryParse(part.TrimEnd('m', 's'), out var milliseconds))
                    {
                        totalMs += (long)milliseconds;
                    }
                }

                if (totalMs > 0)
                {
                    result = TimeSpan.FromMilliseconds(totalMs);
                    return true;
                }
            }
            catch
            {
                // Parsing failed
            }

            result = TimeSpan.Zero;
            return false;
        }
    }
}
