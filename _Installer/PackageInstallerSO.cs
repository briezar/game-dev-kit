using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace GameDevKit.Installer
{
    [CreateAssetMenu(menuName = "GameDevKit/PackageInstaller")]
    internal class PackageInstallerSO : ScriptableObject
    {
        [Tooltip("Required for the project to compile")]
        [SerializeField] private List<PackageEntry> _dependencies = new();

        [Tooltip("Optional but recommended packages")]
        [SerializeField] private List<PackageEntry> _essentials = new();

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
            while (!listRequest.IsCompleted) { await Task.Yield(); }

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
                $"The following recommended packages are not installed:\n\n{GetPackageListing(missingPackages)}\n\nInstall them now?\n\nThis dialog will not show again, you can install from the ${nameof(PackageInstallerSO)}'s Context Menu (top right '...').",
                "Install", "Skip");

            if (install) { PackageUtils.InstallPackages(missingPackages); }
        }
    }
}