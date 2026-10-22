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

        /// <summary>
        /// Returns the type name prefixed by its declaring types.
        /// </summary>
        private static string GetTypeLabel(Type type)
        {
            var parts = new List<string>();
            for (var current = type; current != null; current = current.DeclaringType)
            {
                parts.Insert(0, current.Name);
            }
            return string.Join(".", parts);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var currentType = property.managedReferenceValue?.GetType();
            var buttonContent = currentType == null
                ? new GUIContent("Not set")
                : new GUIContent(GetTypeLabel(currentType), currentType.FullName);

            var rect = position;
            rect.x += EditorGUIUtility.labelWidth + 2;
            rect.width -= EditorGUIUtility.labelWidth + 2;
            rect.height = EditorGUIUtility.singleLineHeight;

            if (EditorGUI.DropdownButton(rect, buttonContent, FocusType.Keyboard))
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
            // Captured up front; the SerializedProperty is not valid once the menu callbacks run.
            var targets = property.serializedObject.targetObjects;
            var propertyPath = property.propertyPath;
            var currentType = property.managedReferenceValue?.GetType();
            var ignoreUnityTypes = attribute is SubclassPickerAttribute { IgnoreUnityTypes: true };

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), currentType == null, () => SetManagedReference(targets, propertyPath, null));

            var types = GetDerivedTypes(GetBaseType())
                        .Where(t => !ignoreUnityTypes || !t.Implements<UnityEngine.Object>())
                        .ToList();

            var duplicateLabels = types.GroupBy(GetTypeLabel)
                                       .Where(g => g.Count() > 1)
                                       .Select(g => g.Key)
                                       .ToHashSet();

            foreach (var derivedType in types)
            {
                var itemLabel = GetTypeLabel(derivedType);
                if (duplicateLabels.Contains(itemLabel))
                {
                    var typeNamespace = string.IsNullOrEmpty(derivedType.Namespace) ? "global" : derivedType.Namespace;
                    itemLabel = $"{itemLabel} ({typeNamespace})";
                }

                var capturedType = derivedType;
                menu.AddItem(new GUIContent(itemLabel), currentType == derivedType, () => SetManagedReference(targets, propertyPath, capturedType));
            }
            return menu;
        }

        /// <summary>
        /// Assigns a new instance of <paramref name="type"/> to every target, or null when <paramref name="type"/> is null.
        /// <br/>Each target receives its own instance.
        /// </summary>
        private static void SetManagedReference(UnityEngine.Object[] targets, string propertyPath, Type type)
        {
            foreach (var target in targets)
            {
                var serializedObject = new SerializedObject(target);
                var property = serializedObject.FindProperty(propertyPath);
                if (property == null) { continue; }

                property.managedReferenceValue = type == null ? null : CreateInstance(type);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private static object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type, true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to create instance of {type.Name} with Activator.CreateInstance.\n{ex}");
                return RuntimeHelpers.GetUninitializedObject(type);
            }
        }

        public static class ManagedReferenceContextualPropertyMenu
        {
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

                var targets = property.serializedObject.targetObjects;
                var propertyPath = property.propertyPath;

                menu.AddItem(new GUIContent($"Copy \"{propertyPath}\" property"), false, () => Copy(targets[0], propertyPath));

                var clip = ReadClipboard(logWarning: false);
                if (clip != null)
                {
                    var clipLabel = GetTypeLabel(clip.Value.type);
                    menu.AddItem(new GUIContent($"Paste \"{clipLabel}\" (Replace)"), false, () => PasteReplace(targets, propertyPath));
                    menu.AddItem(new GUIContent($"Paste \"{clipLabel}\" (Overwrite)"), false, () => PasteOverwrite(targets, propertyPath));
                }
                else
                {
                    menu.AddDisabledItem(kPasteReplaceContent);
                    menu.AddDisabledItem(kPasteOverwriteContent);
                }

                // ---- Duplicate (Deep Copy) for array/list elements ----
                if (TryGetArrayElementContext(propertyPath, out var parentPath, out var elementIndex))
                {
                    menu.AddSeparator(string.Empty);
                    menu.AddItem(kDuplicateDeepCopyContent, false, () =>
                    {
                        foreach (var target in targets)
                        {
                            DuplicateArrayElementDeepCopy(target, parentPath, elementIndex);
                        }
                    });
                }
            }

            private static void Copy(UnityEngine.Object target, string propertyPath)
            {
                var property = new SerializedObject(target).FindProperty(propertyPath);
                var value = property?.managedReferenceValue;
                if (value == null) { return; }

                string json = JsonUtility.ToJson(value);
                string typeName = value.GetType().AssemblyQualifiedName;
                SessionState.SetString(kClipboardKey, $"{typeName}|{json}");
            }

            private static (Type type, string json)? ReadClipboard(bool logWarning = true)
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
                    if (logWarning)
                    {
                        Debug.LogWarning($"Failed to resolve type {typeName} during paste.");
                    }
                    return null;
                }

                return (type, json);
            }

            /// <summary>
            /// Replaces the value on every target with its own instance built from the clipboard.
            /// </summary>
            private static void PasteReplace(UnityEngine.Object[] targets, string propertyPath)
            {
                var clip = ReadClipboard();
                if (clip == null) { return; }

                foreach (var target in targets)
                {
                    var serializedObject = new SerializedObject(target);
                    var property = serializedObject.FindProperty(propertyPath);
                    if (property == null) { continue; }

                    property.managedReferenceValue = JsonUtility.FromJson(clip.Value.json, clip.Value.type);
                    serializedObject.ApplyModifiedProperties();
                }
            }

            /// <summary>
            /// Writes the clipboard fields onto each target's current value when the types match.
            /// <br/>A null value is replaced by the clipboard instance, a different type logs a warning and is skipped.
            /// </summary>
            private static void PasteOverwrite(UnityEngine.Object[] targets, string propertyPath)
            {
                var clip = ReadClipboard();
                if (clip == null) { return; }

                foreach (var target in targets)
                {
                    var serializedObject = new SerializedObject(target);
                    var property = serializedObject.FindProperty(propertyPath);
                    if (property == null) { continue; }

                    var currentValue = property.managedReferenceValue;
                    if (currentValue == null)
                    {
                        property.managedReferenceValue = JsonUtility.FromJson(clip.Value.json, clip.Value.type);
                    }
                    else if (currentValue.GetType() == clip.Value.type)
                    {
                        var merged = JsonUtility.FromJson(JsonUtility.ToJson(currentValue), clip.Value.type);
                        JsonUtility.FromJsonOverwrite(clip.Value.json, merged);
                        property.managedReferenceValue = merged;
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot overwrite on '{target.name}': target type {currentValue.GetType().Name} does not match clipboard type {clip.Value.type.Name}.");
                        continue;
                    }
                    serializedObject.ApplyModifiedProperties();
                }
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

            private static string BuildElementPath(string parentPath, int index) => $"{parentPath}.Array.data[{index}]";

            private static void DuplicateArrayElementDeepCopy(UnityEngine.Object target, string parentPath, int srcIndex)
            {
                var so = new SerializedObject(target);
                so.Update();

                var arrayProp = so.FindProperty(parentPath);
                if (arrayProp == null || !arrayProp.isArray)
                {
                    Debug.LogWarning($"[Duplicate (Deep Copy)] Parent is not an array/list: '{parentPath}'");
                    return;
                }

                if (srcIndex >= arrayProp.arraySize) { return; }

                var srcElementPath = BuildElementPath(parentPath, srcIndex);
                var srcElement = so.FindProperty(srcElementPath);
                if (srcElement == null || srcElement.propertyType != SerializedPropertyType.ManagedReference)
                {
                    Debug.LogWarning($"[Duplicate (Deep Copy)] Source element not found or not managed reference: '{srcElementPath}'");
                    return;
                }

                var srcObj = srcElement.managedReferenceValue;

                // Insert a new element at srcIndex + 1 (Unity duplicates reference by default).
                arrayProp.InsertArrayElementAtIndex(srcIndex);

                var dstElementPath = BuildElementPath(parentPath, srcIndex + 1);
                var dstElement = so.FindProperty(dstElementPath);
                if (dstElement == null)
                {
                    Debug.LogWarning($"[Duplicate (Deep Copy)] Destination element not found: '{dstElementPath}'");
                    return;
                }

                if (srcObj != null)
                {
                    var type = srcObj.GetType();
                    dstElement.managedReferenceValue = JsonUtility.FromJson(JsonUtility.ToJson(srcObj), type);
                }
                else
                {
                    dstElement.managedReferenceValue = null;
                }

                so.ApplyModifiedProperties();
            }

            #endregion Duplicate (Deep Copy) implementation
        }
    }
}