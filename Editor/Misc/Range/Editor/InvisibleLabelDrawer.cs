using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor
{
    /// <summary>
    /// A property drawer that draws an invisible label.
    /// This is useful when you want to display a custom label for elements in a List.
    /// Simply override <see cref="GetLabelText"/> to return the desired label text.
    /// <example>
    /// <code>
    /// [CustomPropertyDrawer(typeof(InventoryItem))]
    /// public class InventoryItemDrawer : InvisibleLabelDrawer
    /// {
    /// protected override string GetLabelText(SerializedProperty property)
    /// {
    ///     var inventoryItem = (InventoryItem)property.boxedValue;
    ///     return $"{inventoryItem.Item?.DisplayName ?? "Invalid Item"} ({inventoryItem.amount})";
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public abstract class InvisibleLabelDrawer : PropertyDrawer
    {
        protected static GUIStyle _invisibleStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _invisibleStyle ??= new GUIStyle(GUIStyle.none) { normal = { textColor = Color.clear } };
            EditorGUI.LabelField(position, GetLabelText(property), _invisibleStyle);
            EditorGUI.PropertyField(position, property, label, true);
        }

        protected abstract string GetLabelText(SerializedProperty property);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}