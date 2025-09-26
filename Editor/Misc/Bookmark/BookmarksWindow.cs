using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameDevKit.Editor
{
    public class BookmarksWindow : EditorWindow
    {
        [MenuItem("Window/General/Bookmarks")]
        public static void Open() => GetWindow<BookmarksWindow>("Bookmarks");

        private Vector2 _scroll;
        private int _selectedTab = 0;
        private readonly Dictionary<BookmarkDatabase.Bookmark, ReorderableList> _lists = new();

        private void OnGUI()
        {
            var db = BookmarkDatabase.instance;
            using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scroll.scrollPosition;
                EditorGUILayout.Space(4);
                DrawTabBar();
                if (db.bookmarks.Count == 0)
                {
                    EditorGUILayout.HelpBox("No bookmark tabs available. Click the '+' button to add a new tab.", MessageType.Info);
                    return;
                }
                _selectedTab = _selectedTab.ClampCollection(db.bookmarks);
                if (!DrawBookmark(db.bookmarks[_selectedTab])) { return; }
            }
        }

        private void DrawTabBar()
        {
            var db = BookmarkDatabase.instance;
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                for (int i = 0; i < db.bookmarks.Count; i++)
                {
                    var tab = db.bookmarks[i];
                    bool isSelected = i == _selectedTab;
                    var style = isSelected ? CustomStyles.selectedToolbarButton : EditorStyles.toolbarButton;

                    if (GUILayout.Toggle(isSelected, tab.name, style, GUILayout.MinWidth(80)))
                    {
                        _selectedTab = i;
                    }
                }

                // "+" button for new tab
                if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    db.bookmarks.Add(new BookmarkDatabase.Bookmark() { name = "New Bookmark" });
                    _selectedTab = db.bookmarks.Count - 1;
                    db.Save();
                }
            }
        }

        private bool DrawBookmark(BookmarkDatabase.Bookmark bookmark)
        {
            var db = BookmarkDatabase.instance;
            bool removeBookmark = false;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // Header
                using (new EditorGUILayout.HorizontalScope(EditorStyles.label))
                {
                    EditorGUI.BeginChangeCheck();
                    var newName = EditorGUILayout.DelayedTextField(bookmark.name, CustomStyles.centeredBoldLabel);
                    if (EditorGUI.EndChangeCheck())
                    {
                        GUI.FocusControl(null);
                    }

                    if (newName != bookmark.name)
                    {
                        bookmark.name = newName;
                        db.Save();
                    }

                    if (GUILayout.Button("Select All", GUILayout.Width(75)))
                    {
                        if (bookmark.objects.Count > 0)
                        {
                            Selection.objects = bookmark.objects.ToArray();
                            for (int i = bookmark.objects.Count - 1; i >= 0; i--)
                            {
                                var obj = bookmark.objects[i];
                                EditorGUIUtility.PingObject(obj);
                            }
                        }
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width(55)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Bookmark",
                            $"Are you sure you want to delete \"{bookmark.name}\"?", "Yes", "Cancel"))
                        {
                            removeBookmark = true; // mark for removal
                        }
                    }
                }

                GetListForBookmark(bookmark).DoLayoutList();
                EditorGUILayout.Space(4);

                // Drag & drop area
                var dropRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
                DrawDashedRect(dropRect, "- Drag objects here -");

                if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform) &&
                    dropRect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (var dragged in DragAndDrop.objectReferences)
                        {
                            bookmark.objects.Add(dragged);
                        }
                        db.Save();
                    }
                    Event.current.Use();
                }
                EditorGUILayout.Space(2);
            }

            if (removeBookmark)
            {
                db.bookmarks.Remove(bookmark);
                db.Save();
                return false;
            }

            return true;
        }

        private ReorderableList GetListForBookmark(BookmarkDatabase.Bookmark bookmark)
        {
            var db = BookmarkDatabase.instance;
            if (_lists.TryGetValue(bookmark, out var list)) { return list; }

            list = new(bookmark.objects, typeof(Object), true, false, false, false);
            list.footerHeight = 0;
            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var obj = bookmark.objects[index];
                if (obj == null)
                {
                    bookmark.objects.RemoveAt(index);
                    return;
                }

                rect.height = EditorGUIUtility.singleLineHeight;

                // shrink rect so we have room for the remove button
                var buttonRect = new Rect(rect.x, rect.y, rect.width - 24, rect.height);
                var removeRect = new Rect(rect.xMax - 20, rect.y, 20, rect.height);

                var content = EditorGUIUtility.ObjectContent(obj, obj.GetType());
                content.text = " " + obj.name;

                if (GUI.Button(buttonRect, content, EditorStyles.objectField))
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }

                if (GUI.Button(removeRect, "-", EditorStyles.miniButton))
                {
                    bookmark.objects.RemoveAt(index);
                    db.Save();
                }
            };

            list.onReorderCallback = l =>
            {
                db.Save();
            };

            _lists[bookmark] = list;
            return list;
        }

        private void DrawDashedRect(Rect rect, string label, float dashLength = 6f, float gapLength = 4f)
        {
            // Background fill
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.03f));

            // Label
            GUI.Label(rect, label, EditorStyles.centeredGreyMiniLabel);

            var lineColor = Color.gray;

            // Top edge
            for (float x = rect.xMin; x < rect.xMax; x += dashLength + gapLength)
            {
                var w = Mathf.Min(dashLength, rect.xMax - x);
                EditorGUI.DrawRect(new Rect(x, rect.yMin, w, 1f), lineColor);
            }

            // Bottom edge
            for (float x = rect.xMin; x < rect.xMax; x += dashLength + gapLength)
            {
                var w = Mathf.Min(dashLength, rect.xMax - x);
                EditorGUI.DrawRect(new Rect(x, rect.yMax - 1f, w, 1f), lineColor);
            }

            // Left edge
            for (float y = rect.yMin; y < rect.yMax; y += dashLength + gapLength)
            {
                var h = Mathf.Min(dashLength, rect.yMax - y);
                EditorGUI.DrawRect(new Rect(rect.xMin, y, 1f, h), lineColor);
            }

            // Right edge
            for (float y = rect.yMin; y < rect.yMax; y += dashLength + gapLength)
            {
                var h = Mathf.Min(dashLength, rect.yMax - y);
                EditorGUI.DrawRect(new Rect(rect.xMax - 1f, y, 1f, h), lineColor);
            }
        }

        public static class CustomStyles
        {
            private static GUIStyle _centeredBoldLabel;
            public static GUIStyle centeredBoldLabel => _centeredBoldLabel ??= new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };

            private static GUIStyle _selectedToolbarButton;
            public static GUIStyle selectedToolbarButton => _selectedToolbarButton ??= new GUIStyle(EditorStyles.toolbarButton)
            {
                normal = { background = Texture2D.grayTexture },
                fontStyle = FontStyle.Bold
            };
        }

    }
}