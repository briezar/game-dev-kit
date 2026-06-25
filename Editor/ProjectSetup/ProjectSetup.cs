using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameDevKit.Editor
{
    public static class ProjectSetup
    {
        private const string MenuItemPath = EditorConstants.MenuItemPath + "Project Setup/";

        [MenuItem(MenuItemPath + "Create Folder Structure")]
        public static void CreateFolderStructure()
        {
            DirectoryUtils.CreateFolder("_Project", new[] { "Art", "Audio", "Prefabs", "Presets", "Scripts" });
            DirectoryUtils.CreateFolder("_Project/Art", new[] { "Fonts", "Materials", "Shaders", "Sprites" });

            DirectoryUtils.MoveInto("_Project", "Scenes");
            DirectoryUtils.MoveInto("Settings", "InputSystem_Actions.inputactions");

            DirectoryUtils.DeleteFolder("TutorialInfo");
            DirectoryUtils.DeleteFile("Readme.asset");

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
                { "-nowarn:0067", "CS0067: An event was declared but never used in the class in which it was declared" },
            };

            var cscPath = Path.Combine(Application.dataPath, "csc.rsp");
            var cscSuppressions = File.Exists(cscPath) ? File.ReadAllLines(cscPath).ToHashSet() : new HashSet<string>();
            cscSuppressions.UnionWith(argumentMap.Keys);

            File.WriteAllLines(cscPath, cscSuppressions);

            var msg = $"Added compiler suppressions through {cscPath}:\n";
            Debug.Log(msg + argumentMap.JoinToString("{0} ({1})"));
        }
    }
}