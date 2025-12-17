using System;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;
using GameDevKit.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GameDevKit.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(SubclassPickerAttribute))]
    public class SubclassPickerDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, List<Type>> _typeCache = new();
        private static List<Type> GetDerivedTypes(Type baseType)
        {
            if (!_typeCache.TryGetValue(baseType, out var list))
            {
                list = TypeCache.GetTypesDerivedFrom(baseType)
                                .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false })
                                .ToList();

                if (baseType is { IsAbstract: false, IsGenericTypeDefinition: false })
                {
                    list.AddFirst(baseType);
                }
                _typeCache[baseType] = list;
            }
            return list;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var typeName = property.managedReferenceValue?.GetType().Name ?? "Not set";

            var rect = position;
            rect.x += EditorGUIUtility.labelWidth + 2;
            rect.width -= EditorGUIUtility.labelWidth + 2;
            rect.height = EditorGUIUtility.singleLineHeight;

            if (EditorGUI.DropdownButton(rect, new(ObjectNames.NicifyVariableName(typeName)), FocusType.Keyboard))
            {
                var menu = CreateContextMenu(property);
                menu.DropDown(rect);
            }

            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, position.height), property, label, true);
        }

        private Type GetBaseType()
        {
            var type = fieldInfo.FieldType;

            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        private GenericMenu CreateContextMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), property.managedReferenceValue == null, () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            var currentType = property.managedReferenceValue?.GetType();
            foreach (Type derivedType in GetDerivedTypes(GetBaseType()))
            {
                if (attribute is SubclassPickerAttribute subclassPickerAttribute && subclassPickerAttribute.IgnoreUnityTypes)
                {
                    if (derivedType.Implements<UnityEngine.Object>()) { continue; }
                }

                menu.AddItem(new(ObjectNames.NicifyVariableName(derivedType.Name)), currentType == derivedType, () =>
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

        public static class ManagedReferenceContextualPropertyMenu
        {
            private const string kCopiedPropertyPathKey = "SerializeReference.CopiedPropertyPath";
            private const string kClipboardKey = "SerializeReference.CopyAndPasteProperty";

            private static readonly GUIContent kPasteReplaceContent = new("Paste (Replace)");
            private static readonly GUIContent kPasteOverwriteContent = new("Paste (Overwrite)");
            private static readonly GUIContent kDuplicateDeepCopyContent = new("Duplicate (Deep Copy)");

            // captures index and whether there is a trailing dot (nested)
            private static readonly Regex s_arrayIndexRegex = new(@"\.Array\.data\[(\d+)\](?:$|(\.))", RegexOptions.Compiled);

            [InitializeOnLoadMethod]
            private static void Initialize()
            {
                EditorApplication.contextualPropertyMenu += OnContextualPropertyMenu;
            }

            private static void OnContextualPropertyMenu(GenericMenu menu, SerializedProperty property)
            {
                if (property.propertyType != SerializedPropertyType.ManagedReference) { return; }

                var clonedProperty = property.Copy();
                menu.AddItem(new GUIContent($"Copy \"{property.propertyPath}\" property"), false, Copy, clonedProperty);

                string copiedPropertyPath = SessionState.GetString(kCopiedPropertyPathKey, string.Empty);
                if (!string.IsNullOrEmpty(copiedPropertyPath))
                {
                    menu.AddItem(new GUIContent($"Paste \"{copiedPropertyPath}\" (Replace)"), false, PasteReplace, clonedProperty);
                    menu.AddItem(new GUIContent($"Paste \"{copiedPropertyPath}\" (Overwrite)"), false, PasteOverwrite, clonedProperty);
                }
                else
                {
                    menu.AddDisabledItem(kPasteReplaceContent);
                    menu.AddDisabledItem(kPasteOverwriteContent);
                }

                // ---- Duplicate (Deep Copy) for array/list elements ----
                if (TryGetArrayElementContext(property.propertyPath, out var parentPath, out var elementIndex))
                {
                    var target = property.serializedObject.targetObject;
                    var elementPath = BuildElementPath(parentPath, elementIndex);

                    menu.AddSeparator(string.Empty);
                    menu.AddItem(kDuplicateDeepCopyContent, false, () =>
                    {
                        DuplicateArrayElementDeepCopy(target, parentPath, elementIndex, elementPath);
                    });
                }
            }

            private static void Copy(object customData)
            {
                var property = (SerializedProperty)customData;
                var value = property.managedReferenceValue;
                if (value == null) { return; }

                string json = JsonUtility.ToJson(value);
                string typeName = value.GetType().AssemblyQualifiedName;

                SessionState.SetString(kCopiedPropertyPathKey, property.propertyPath);
                SessionState.SetString(kClipboardKey, $"{typeName}|{json}");
            }

            private static (Type type, string json)? ReadClipboard()
            {
                var clipboard = SessionState.GetString(kClipboardKey, string.Empty);
                if (string.IsNullOrEmpty(clipboard)) { return null; }

                var split = clipboard.Split(new[] { '|' }, 2);
                if (split.Length != 2) { return null; }

                var typeName = split[0];
                var json = split[1];
                var type = Type.GetType(typeName);
                if (type == null)
                {
                    Debug.LogWarning($"Failed to resolve type {typeName} during paste.");
                    return null;
                }

                return (type, json);
            }

            private static void PasteReplace(object customData)
            {
                var property = (SerializedProperty)customData;
                var clip = ReadClipboard();
                if (clip == null) { return; }

                // this actually does not work for multi-selected array elements
                // property.serializedObject.targetObjects only return the currently selected object
                // foreach (var target in property.serializedObject.targetObjects)
                // {
                // }

                var sObj = property.serializedObject;
                var sProp = sObj.FindProperty(property.propertyPath);
                if (sProp == null) { return; }

                object newValue = JsonUtility.FromJson(clip.Value.json, clip.Value.type);
                Undo.RecordObject(sObj.targetObject, "Paste Property (Replace)");
                sProp.managedReferenceValue = newValue;
                sObj.ApplyModifiedProperties();
            }

            private static void PasteOverwrite(object customData)
            {
                var property = (SerializedProperty)customData;
                var clip = ReadClipboard();
                if (clip == null) { return; }

                var sObj = property.serializedObject;
                var sProp = sObj.FindProperty(property.propertyPath);
                if (sProp == null) { return; }

                var currentValue = sProp.managedReferenceValue;
                if (currentValue == null)
                {
                    object newValue = JsonUtility.FromJson(clip.Value.json, clip.Value.type);
                    Undo.RecordObject(sObj.targetObject, "Paste Property (Overwrite → Replace)");
                    sProp.managedReferenceValue = newValue;
                }
                else if (currentValue.GetType() == clip.Value.type)
                {
                    Undo.RecordObject(sObj.targetObject, "Paste Property (Overwrite)");
                    JsonUtility.FromJsonOverwrite(clip.Value.json, currentValue);
                }
                else
                {
                    Debug.LogWarning($"Cannot overwrite: target type {currentValue.GetType().Name} does not match clipboard type {clip.Value.type.Name}.");
                }
                sObj.ApplyModifiedProperties();
            }

            #region Duplicate (Deep Copy) implementation
            private static bool TryGetArrayElementContext(string propertyPath, out string parentPath, out int index)
            {
                // Works whether right-click was on the element itself "...Array.data[n]"
                // or on any nested child "...Array.data[n].child.more"
                var m = s_arrayIndexRegex.Match(propertyPath);
                if (!m.Success)
                {
                    parentPath = null;
                    index = -1;
                    return false;
                }

                // Trim to the element path (remove nested tail if any)
                int cut = propertyPath.IndexOf(".Array.data[", StringComparison.Ordinal);
                parentPath = propertyPath.Substring(0, cut);
                index = int.Parse(m.Groups[1].Value);
                return true;
            }

            private static string BuildElementPath(string parentPath, int index)
            {
                return $"{parentPath}.Array.data[{index}]";
            }

            private static void DuplicateArrayElementDeepCopy(UnityEngine.Object target, string parentPath, int srcIndex, string srcElementPath)
            {
                var so = new SerializedObject(target);
                so.Update();

                var arrayProp = so.FindProperty(parentPath);
                if (arrayProp == null || !arrayProp.isArray)
                {
                    Debug.LogWarning($"[Duplicate (Deep Copy)] Parent is not an array/list: '{parentPath}'");
                    return;
                }

                var srcElement = so.FindProperty(srcElementPath);
                if (srcElement == null || srcElement.propertyType != SerializedPropertyType.ManagedReference)
                {
                    Debug.LogWarning($"[Duplicate (Deep Copy)] Source element not found or not managed reference: '{srcElementPath}'");
                    return;
                }

                Undo.RecordObject(target, "Duplicate Array Element (Deep Copy)");

                // Insert a new element at srcIndex + 1 (Unity duplicates reference by default).
                arrayProp.InsertArrayElementAtIndex(srcIndex);

                // Resolve destination element and replace with a real deep copy.
                var dstIndex = srcIndex + 1;
                var dstElementPath = BuildElementPath(parentPath, dstIndex);
                var dstElement = so.FindProperty(dstElementPath);
                if (dstElement == null)
                {
                    Debug.LogWarning($"[Duplicate (Deep Copy)] Destination element not found: '{dstElementPath}'");
                    return;
                }

                var srcObj = srcElement.managedReferenceValue;
                if (srcObj != null)
                {
                    var type = srcObj.GetType();
                    var json = JsonUtility.ToJson(srcObj);
                    var clone = JsonUtility.FromJson(json, type);
                    dstElement.managedReferenceValue = clone;
                }
                else
                {
                    // Keep null if source was null
                    dstElement.managedReferenceValue = null;
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }

            #endregion Duplicate (Deep Copy) implementation
        }

    }

}