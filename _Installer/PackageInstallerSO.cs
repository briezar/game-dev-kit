using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditorInternal;
using UnityEngine;

namespace GameDevKit.Installer
{
    [CreateAssetMenu(menuName = "GameDevKit/PackageInstaller")]
    internal class PackageInstallerSO : ScriptableObject
    {
        [Header("Check the Context Menu for commands")]
        [Space]
        [Tooltip("Required for the project to compile")]
        [SerializeField] private List<PackageEntry> _dependencies = new();

        [Tooltip("Optional but recommended packages")]
        [SerializeField] private List<PackageEntry> _essentials = new();

        [Tooltip("Asmdefs that should remain excluded from compilation until PackageInstaller has installed their required dependencies. The Update Dependent Asmdefs command adds the necessary version defines and constraints.")]
        [SerializeField] private AssemblyDefinitionAsset[] _dependentAsmdefs = Array.Empty<AssemblyDefinitionAsset>();

        public IReadOnlyList<PackageEntry> Dependencies => _dependencies;
        public IReadOnlyList<PackageEntry> Essentials => _essentials;

        internal static PackageInstallerSO Load()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(PackageInstallerSO)}");
            if (guids.Length == 0) { return null; }
            return AssetDatabase.LoadAssetAtPath<PackageInstallerSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        [ContextMenu("Install Packages")]
        private void InstallPackages() => InstallationChecker.CheckDependencies();

        [ContextMenu("Update Dependent Asmdefs")]
        private void UpdateDependentAsmdefs()
        {
            foreach (var asmdef in _dependentAsmdefs)
            {
                if (asmdef == null) { continue; }

                var prevJson = asmdef.text;
                var data = JsonUtility.FromJson<AssemblyDefinitionData>(prevJson);
                if (data == null) { continue; }

                data.versionDefines ??= new();
                data.defineConstraints ??= new();

                foreach (var packageId in _dependencies.Select(dependency => dependency.packageId).Where(packageId => !string.IsNullOrEmpty(packageId)))
                {
                    var define = GetDefineSymbol(packageId);
                    var versionDefine = data.versionDefines.FirstOrDefault(item => item.name == packageId);
                    if (versionDefine == null)
                    {
                        data.versionDefines.Add(new()
                        {
                            name = packageId,
                            expression = "1.0.0",
                            define = define
                        });
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(versionDefine.expression))
                        {
                            versionDefine.expression = "1.0.0";
                        }

                        versionDefine.define = define;
                    }

                    if (!data.defineConstraints.Contains(define))
                    {
                        data.defineConstraints.Add(define);
                    }
                }

                var newJson = JsonUtility.ToJson(data, true);
                if (string.Equals(prevJson, newJson, StringComparison.Ordinal))
                {
                    Debug.Log($"Nothing changed in {asmdef.name}", asmdef);
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(asmdef);
                File.WriteAllText(path, newJson);
                AssetDatabase.ImportAsset(path);

                Debug.Log($"Updated {asmdef.name}", asmdef);
            }

        }

        /// <summary>Converts a UPM package ID into a valid scripting define symbol.</summary>
        /// <param name="packageId">The package ID used by the version define.</param>
        /// <returns><c>DEPENDENCY_[x]</c> symbol derived from the package ID.</returns>
        private static string GetDefineSymbol(string packageId)
        {
            const string removeStart = "com.";
            if (packageId.StartsWith(removeStart, StringComparison.Ordinal))
            {
                packageId = packageId[removeStart.Length..];
            }

            var define = new string(packageId
                .ToUpperInvariant()
                .Select(character => character is >= 'A' and <= 'Z' || character is >= '0' and <= '9' || character == '_' ? character : '_')
                .ToArray());

            return "DEPENDENCY_" + define;
        }
    }

    [InitializeOnLoad]
    internal static class InstallationChecker
    {
        private const string DepsValidatedKey = "GameDevKit_DepsValidated";
        private const string EssentialsPromptedKey = "GameDevKit_EssentialsPrompted";

        static InstallationChecker()
        {
            // Invalidate the deps check whenever packages are added/removed,
            // so the next domain reload re-runs the check.
            Events.registeringPackages += _ => EditorPrefs.DeleteKey(DepsValidatedKey);

            if (!EditorPrefs.GetBool(DepsValidatedKey)) { EditorApplication.delayCall += CheckDependencies; }
        }

        internal static async void CheckDependencies()
        {
            var so = PackageInstallerSO.Load();
            if (so == null)
            {
                Debug.LogWarning("[GameDevKit] No PackageInstallerSO found in project. Skipping dependency check.");
                return;
            }

            var listRequest = Client.List();
            var timer = new Timer(1f);
            while (!listRequest.IsCompleted)
            {
                EditorUtility.DisplayProgressBar(
                    "GameDevKit — Checking Packages",
                    "Loading installed packages...",
                    timer.GetProgressMax(0.9f));
                await Task.Delay(timer.IntervalMs);
                timer.Tick();
            }

            EditorUtility.ClearProgressBar();

            if (listRequest.Status == StatusCode.Failure)
            {
                Debug.LogError("[GameDevKit] Could not list packages: " + listRequest.Error.message);
                return;
            }

            var installedIds = listRequest.Result.Select(p => p.name).ToHashSet();

            var missingDeps = so.Dependencies
                .Where(p => !installedIds.Contains(p.packageId))
                .ToList();

            var missingEssentials = so.Essentials
                .Where(p => !installedIds.Contains(p.packageId))
                .ToList();

            PromptInstallDependencies(missingDeps);

            EditorPrefs.DeleteKey(EssentialsPromptedKey);
            PromptInstallEssentials(missingEssentials);

            // Only suppress future checks if all required deps are satisfied.
            if (missingDeps.Count == 0) { EditorPrefs.SetBool(DepsValidatedKey, true); }
        }

        internal static void PromptInstallEssentials()
        {
            // Called from ContextMenu — reset opt-out so the dialog always shows regardless of prior choice.
            EditorApplication.delayCall += CheckDependencies;
        }

        private static string GetPackageListing(List<PackageEntry> packages)
        {
            return string.Join("\n", packages.Select(p => $"  • {p.packageId}"));
        }

        private static void PromptInstallDependencies(List<PackageEntry> missingPackages)
        {
            if (missingPackages.Count == 0) { return; }

            var install = EditorUtility.DisplayDialog(
                "GameDevKit — Missing Dependencies",
                $"The following required packages are not installed:\n\n{GetPackageListing(missingPackages)}\n\nInstall them now?",
                "Install", "Ignore");

            if (install) { PackageUtils.InstallPackages(missingPackages); }
        }

        private static void PromptInstallEssentials(List<PackageEntry> missingPackages)
        {
            if (EditorPrefs.GetBool(EssentialsPromptedKey)) { return; }
            EditorPrefs.SetBool(EssentialsPromptedKey, true);

            if (missingPackages.Count == 0) { return; }

            var install = EditorUtility.DisplayDialog(
                "GameDevKit — Recommended Packages",
                $"The following recommended packages are not installed:\n\n{GetPackageListing(missingPackages)}\n\nInstall them now?\n\nThis dialog will not show again, you can install from the {nameof(PackageInstallerSO)}'s Context Menu (top right '...').",
                "Install", "Skip");

            if (install) { PackageUtils.InstallPackages(missingPackages); }
        }
    }
}