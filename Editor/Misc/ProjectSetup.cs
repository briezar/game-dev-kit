using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace GameDevKit.Editor
{
    public static class ProjectSetup
    {
        private const string MenuItemPath = EditorConstants.MenuItemPath + "Project Setup/";

        [MenuItem(MenuItemPath + "Install Essential Packages")]
        public static void InstallEssentialPackages()
        {
            PackagesHelper.InstallPackages(
                "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
                "https://github.com/dbrizov/NaughtyAttributes.git#upm",
                "https://github.com/yasirkula/UnityAssetUsageDetector.git",
                "https://github.com/yasirkula/UnityIngameDebugConsole.git",
                "com.unity.nuget.newtonsoft-json"
            );
        }

        [MenuItem(MenuItemPath + "Create Folder Structure")]
        public static void CreateFolderStructure()
        {
            AssetsDirectory.CreateFolder("_Project", new[] { "Art", "Audio", "Prefabs", "Presets", "Scripts" });
            AssetsDirectory.CreateFolder("_Project/Art", new[] { "Fonts", "Materials", "Shaders", "Sprites" });

            AssetsDirectory.MoveInto("_Project", "Scenes");
            AssetsDirectory.MoveInto("Settings", "InputSystem_Actions.inputactions");

            AssetsDirectory.DeleteFolder("TutorialInfo");
            AssetsDirectory.DeleteFile("Readme.asset");

            AssetDatabase.Refresh();

            // Optional: Disable Domain Reload
            // EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        }

        [MenuItem(MenuItemPath + "Add Compiler Suppressions")]
        public static void AddCompilerSuppressions()
        {
            var argumentMap = new Dictionary<string, string>()
        {
            { "-nowarn:0414", "CS0414: The private field 'field' is assigned but its value is never used" },
            { "-nowarn:4014", "CS4014: Because this call is not awaited, execution of the current method continues before the call is completed" },
            { "-nowarn:1998", "CS1998: Async method lacks 'await' operators and will run synchronously" },
        };

            var cscPath = Path.Combine(Application.dataPath, "csc.rsp");
            var cscSuppressions = File.Exists(cscPath) ? File.ReadAllLines(cscPath).ToHashSet() : new HashSet<string>();
            cscSuppressions.UnionWith(argumentMap.Keys);

            File.WriteAllLines(cscPath, cscSuppressions);

            var msg = "Added compiler suppressions through Assets/csc.rsp:\n";
            Debug.Log(msg + argumentMap.JoinToString("{0} ({1})"));
        }

        private static class PackagesHelper
        {
            private static readonly Queue<string> packagesToInstall = new();

            public static void InstallPackages(params string[] packages)
            {
                foreach (var package in packages)
                {
                    packagesToInstall.Enqueue(package);
                }

                if (packagesToInstall.Count > 0)
                {
                    StartNextPackageInstallation();
                }
            }

            private static async void StartNextPackageInstallation()
            {
                var request = Client.Add(packagesToInstall.Dequeue());

                while (!request.IsCompleted) await Task.Delay(100);

                if (request.Status == StatusCode.Success) Debug.Log("Installed: " + request.Result.packageId);
                else if (request.Status >= StatusCode.Failure) Debug.LogError(request.Error.message);

                if (packagesToInstall.Count > 0)
                {
                    await Task.Delay(100);
                    StartNextPackageInstallation();
                }
            }
        }

        private static class AssetsDirectory
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

            private static string ToRelativePath(params string[] paths)
            {
                return Path.Combine(Application.dataPath, Path.Combine(paths));
            }

        }
    }
}