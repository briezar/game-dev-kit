using System;
using System.IO;
using UnityEngine;

namespace GameDevKit.Editor
{
    public static class DirectoryUtils
    {
        public static void CreateFolder(string folderPath, string[] childrenFolders = null)
        {
            folderPath = ToRelativePath(folderPath);
            try
            {
                Directory.CreateDirectory(folderPath);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            if (childrenFolders == null) { return; }

            foreach (var children in childrenFolders)
            {
                CreateFolder(Path.Combine(folderPath, children), null);
            }
        }


        public static void MoveInto(string parent, string sourcePath)
        {
            var sourceAbsolutePath = ToRelativePath(sourcePath);
            var dest = ToRelativePath(parent, sourcePath);

            try
            {
                Directory.Move(sourceAbsolutePath, dest);
                File.Move(sourceAbsolutePath + ".meta", dest + ".meta");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public static void DeleteFolder(string path)
        {
            path = ToRelativePath(path);

            try
            {
                Directory.Delete(path, true);
                File.Delete(path + ".meta");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public static void DeleteFile(string path)
        {
            path = ToRelativePath(path);

            try
            {
                File.Delete(path);
                File.Delete(path + ".meta");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public static string ToRelativePath(params string[] paths) => Path.Combine(Application.dataPath, Path.Combine(paths));

    }
}
