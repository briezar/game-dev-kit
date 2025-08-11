using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(SerializableType))]
    public class SerializableTypeDrawer : PropertyDrawer
    {
        private SerializedProperty _assemblyQualifiedNameProp;
        private TypeFilterAttribute _typeFilter;

        private bool _isFirstUpdate = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_isFirstUpdate)
            {
                _assemblyQualifiedNameProp = property.FindPropertyRelative(SerializableType.EditorProps.AssemblyQualifiedName);
                _typeFilter = (TypeFilterAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(TypeFilterAttribute)) ?? new TypeFilterAttribute(null);
            }

            var selectedType = Type.GetType(_assemblyQualifiedNameProp.stringValue);
            var isInvalidType = selectedType != null && !_typeFilter.IsValidType(selectedType);
            var btnStyle = new GUIStyle(EditorStyles.miniPullDown);

            if (isInvalidType)
            {
                if (_isFirstUpdate)
                {
                    Debug.LogWarning($"Type '{selectedType.Name}' is not a valid type for {fieldInfo.Name} based on {nameof(TypeFilterAttribute)}.", property.serializedObject.targetObject);
                }

                btnStyle.normal.textColor = Color.yellowNice;
                btnStyle.hover.textColor = Color.yellowNice;
                btnStyle.active.textColor = Color.yellowNice;
            }

            position = EditorGUI.PrefixLabel(position, label);

            if (EditorGUI.DropdownButton(position, new GUIContent(selectedType != null ? $"{selectedType.Name} ({selectedType.FullName})" : "None"), FocusType.Keyboard, btnStyle))
            {
                if (SerializableTypePopup.IsOpened) { return; }
                SerializableTypePopup.Open(position, selectedType, _typeFilter, t =>
                {
                    _assemblyQualifiedNameProp.stringValue = t.AssemblyQualifiedName;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            _isFirstUpdate = false;
        }
    }

    public class SerializableTypePopup : EditorWindow
    {
        private const float PopupHeight = 250f;
        private const float RowHeight = 20f;
        private const string SearchFieldGUIControl = "SearchField";

        private TypeFilterAttribute _typeFilter;
        private static List<Type> _allTypes;
        private readonly List<Type> _filteredTypes = new();
        private Vector2 _scroll;
        private string _searchTerm = "";
        private Type _selectedType;
        private Action<Type> _onSelect;
        private int _hoverIndex = -1;
        private int _pendingScrollIndex = -1;

        private bool _isFirstUpdate = true;

        public static bool IsOpened { get; private set; }

        private static GUIStyle _listItemStyle;
        private static GUIStyle _selectedItemStyle;
        private static GUIStyle _searchStyle;
        private static GUIStyle _cancelButtonStyle;

        public static void Open(Rect activatorRect, Type selectedType, TypeFilterAttribute typeFilter, Action<Type> onSelect)
        {
            _allTypes ??= LoadAllProjectTypes();
            InitStyles();

            var win = CreateInstance<SerializableTypePopup>();
            win._selectedType = selectedType;
            win._typeFilter = typeFilter ?? new TypeFilterAttribute(null);
            win._onSelect = onSelect;
            win.FilterTypes();

            if (selectedType != null)
            {
                win._pendingScrollIndex = win._filteredTypes.IndexOf(selectedType);
                if (win._pendingScrollIndex >= 0)
                {
                    win._hoverIndex = win._pendingScrollIndex;
                }
            }

            var screenRect = GUIUtility.GUIToScreenRect(activatorRect);
            win.ShowAsDropDown(screenRect, new Vector2(EditorGUIUtility.currentViewWidth - 20, PopupHeight));
        }

        private void OnEnable()
        {
            IsOpened = true;
        }

        private void OnDestroy()
        {
            EditorApplication.delayCall += () =>
            {
                IsOpened = false;
            };
        }

        private void OnGUI()
        {
            // Draw background & outline
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), new Color(0.22f, 0.22f, 0.22f));
            GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none, EditorStyles.helpBox);

            DrawSearchBar();

            var listRect = new Rect(0, EditorGUIUtility.singleLineHeight + 6, position.width, position.height - EditorGUIUtility.singleLineHeight - 6);
            var contentRect = new Rect(0, 0, listRect.width - 16, _filteredTypes.Count * RowHeight);

            // Scroll to selected element on first draw
            if (_pendingScrollIndex >= 0)
            {
                float targetY = _pendingScrollIndex * RowHeight - (listRect.height - RowHeight) / 2;
                _scroll.y = Mathf.Clamp(targetY, 0, Mathf.Max(0, contentRect.height - listRect.height));
                _pendingScrollIndex = -1;
            }

            _scroll = GUI.BeginScrollView(listRect, _scroll, contentRect);

            for (int i = 0; i < _filteredTypes.Count; i++)
            {
                var t = _filteredTypes[i];
                var itemRect = new Rect(0, i * RowHeight, contentRect.width, RowHeight);

                if (itemRect.Contains(Event.current.mousePosition))
                    _hoverIndex = i;

                var style = (_selectedType == t || _hoverIndex == i) ? _selectedItemStyle : _listItemStyle;

                if (GUI.Button(itemRect, $"{t.Name} ({t.FullName})", style))
                {
                    _onSelect?.Invoke(t);
                    Close();
                }
            }

            GUI.EndScrollView();

            _isFirstUpdate = false;

        }

        private void DrawSearchBar()
        {
            var searchRect = new Rect(4, 4, position.width - 8, EditorGUIUtility.singleLineHeight);
            var searchTextRect = searchRect;
            searchTextRect.width -= _cancelButtonStyle.fixedWidth;

            GUI.SetNextControlName(SearchFieldGUIControl);
            _searchTerm = EditorGUI.TextField(searchTextRect, _searchTerm, _searchStyle);
            if (_isFirstUpdate)
            {
                GUI.FocusControl(SearchFieldGUIControl);
            }

            var cancelRect = new Rect(searchTextRect.xMax, searchTextRect.y, _cancelButtonStyle.fixedWidth, searchTextRect.height);
            if (GUI.Button(cancelRect, GUIContent.none, _cancelButtonStyle))
            {
                _searchTerm = "";
                GUI.FocusControl(SearchFieldGUIControl);
                FilterTypes();
            }

            if (Event.current.type is EventType.KeyDown or EventType.KeyUp)
            {
                FilterTypes();
            }
        }

        private void FilterTypes()
        {
            _filteredTypes.Clear();
            if (string.IsNullOrEmpty(_searchTerm))
            {
                _filteredTypes.AddRange(_allTypes.Where(t => _typeFilter.IsValidType(t)));
            }
            else
            {
                bool MatchesSearchTerm(string value) => value.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase);

                _filteredTypes.AddRange(_allTypes.Where(t => _typeFilter.IsValidType(t) && (MatchesSearchTerm(t.Name) || MatchesSearchTerm(t.FullName))));
            }
        }

        private static void InitStyles()
        {
            _searchStyle ??= GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.textField;
            _cancelButtonStyle ??= GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? new GUIStyle(GUI.skin.button) { fixedWidth = 18 };
            _listItemStyle ??= new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(4, 4, 2, 2) };
            _selectedItemStyle ??= new GUIStyle(_listItemStyle)
            {
                normal = { background = Texture2D.grayTexture, textColor = Color.white }
            };
        }

        private static List<Type> LoadAllProjectTypes()
        {
            var excludedAssembliesStartWith = new[] { "Unity", "Mono", "Bee", "System", "Microsoft", "Google", "Firebase" };
            var excludedAssemblies = new HashSet<string> { "Assembly-CSharp-Editor", "Assembly-CSharp-Editor-firstpass", "mscorlib", "netstandard", "PsdPlugin", "ScriptablePacker" };
            var excludedTypesStartWith = new[] { "$", "<", "UnitySourceGenerated" };
            var excludedTypesContain = new[] { "Editor" };

            var types = TypeCache.GetTypesDerivedFrom<object>()
                .Where(t =>
                {
                    var assemblyName = t.Assembly.GetName().Name;
                    foreach (var excluded in excludedAssembliesStartWith)
                    {
                        if (assemblyName.StartsWith(excluded)) { return false; }
                    }

                    if (excludedAssemblies.Contains(assemblyName)) { return false; }

                    foreach (var excluded in excludedTypesStartWith)
                    {
                        if (t.FullName.StartsWith(excluded)) { return false; }
                    }

                    foreach (var excluded in excludedTypesContain)
                    {
                        if (t.FullName.Contains(excluded)) { return false; }
                    }

                    return true;
                })
                .OrderBy(t => t.FullName)
                .ToList();

            return types;
        }
    }
}