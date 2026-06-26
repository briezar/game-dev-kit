using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace GameDevKit.Installer
{
    public static class PackageUtils
    {
        public static async void InstallPackages(IEnumerable<PackageEntry> packages)
        {
            EnsureScopedRegistry(packages);

            var packagesToAdd = packages.Select(p => p.sourceType is PackageSourceType.Git ? p.gitUrl : p.packageId).ToList();

            var request = Client.AddAndRemove(packagesToAdd.ToArray());

            while (!request.IsCompleted)
            {
                await Task.Yield();
            }

            var installedPackages = request.Result.Where(p => packagesToAdd.Contains(p.name, StringComparer.Ordinal)).ToList();

            if (request.Status == StatusCode.Success)
            {
                Debug.Log($"[GameDevKit] Installed {installedPackages.Count} packages:\n{string.Join("\n", installedPackages.Select(p => p.packageId))}");
            }
            else if (request.Status >= StatusCode.Failure)
            {
                Debug.LogError($"[GameDevKit] Failed to install {installedPackages.Count}: {request.Error.message}");
            }
        }

        // Scoped registries live in Packages/manifest.json and must be added before Client.Add()
        private static void EnsureScopedRegistry(IEnumerable<PackageEntry> packages)
        {
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            var manifest = JsonConvert.DeserializeObject<PackageManifest>(File.ReadAllText(manifestPath));

            foreach (var package in packages)
            {
                if (package.sourceType is not PackageSourceType.ScopedRegistry) { continue; }
                if (manifest.scopedRegistries.Exists(r => string.Equals(r.url, package.scopedRegistry.url, StringComparison.Ordinal))) { continue; }

                manifest.scopedRegistries.Add(package.scopedRegistry);
                File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
                Debug.Log($"[GameDevKit] Added scoped registry: {package.scopedRegistry.name} ({package.scopedRegistry.url})");
            }
        }

        public class PackageManifest
        {
            public Dictionary<string, string> dependencies = new();
            public List<ScopedRegistry> scopedRegistries = new();
        }
    }
}