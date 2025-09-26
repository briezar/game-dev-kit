using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameDevKit.Editor
{
    [FilePath("UserSettings/Bookmarks.asset", FilePathAttribute.Location.ProjectFolder)]
    public class BookmarkDatabase : ScriptableSingleton<BookmarkDatabase>
    {
        public List<Bookmark> bookmarks = new();

        [Serializable]
        public class Bookmark
        {
            public string name;
            public List<Object> objects = new();
        }

        public void Save() => Save(true);
    }
}