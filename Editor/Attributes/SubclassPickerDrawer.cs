using System;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;

namespace GameDevKit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(SubclassPickerAttribute))]
    public class SubclassPickerDrawer : PropertyDrawer
    {
        public new SubclassPickerAttribute attribute => (SubclassPickerAttribute)base.attribute;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var type = fieldInfo.FieldType;
            var typeName = property.managedReferenceValue?.GetType().Name ?? "Not set";
            var niceTypeName = ObjectNames.NicifyVariableName(typeName);
            var rect = position;
            rect.x += EditorGUIUtility.labelWidth + 2;
            rect.width -= EditorGUIUtility.labelWidth + 2;
            rect.height = EditorGUIUtility.singleLineHeight;

            if (EditorGUI.DropdownButton(rect, new(niceTypeName), FocusType.Keyboard))
            {
                var menu = CreateContextMenu(type, typeName, property);
                menu.DropDown(rect);
            }

            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, position.height), property, label, true);
        }

        private GenericMenu CreateContextMenu(Type type, string typeName, SerializedProperty property)
        {
            var menu = new GenericMenu();
            foreach (Type derivedType in TypeCache.GetTypesDerivedFrom(type))
            {
                menu.AddItem(new(ObjectNames.NicifyVariableName(derivedType.Name)), typeName == derivedType.Name, () =>
                {
                    // Supports multi-object editing
                    foreach (var targetObject in property.serializedObject.targetObjects)
                    {
                        var individualObject = new SerializedObject(targetObject);
                        var individualProperty = individualObject.FindProperty(property.propertyPath);
                        CreateAndSetManagedReference(individualProperty, derivedType);
                    }

                });
            }
            return menu;
        }

        private void CreateAndSetManagedReference(SerializedProperty property, Type type)
        {
            object instance;
            try
            {
                instance = Activator.CreateInstance(type, true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create instance of {type.Name} with Activator.CreateInstance.\n{ex}");
                instance = RuntimeHelpers.GetUninitializedObject(type);
            }
            property.managedReferenceValue = instance;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}