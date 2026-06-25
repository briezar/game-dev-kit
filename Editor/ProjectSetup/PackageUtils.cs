using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace GameDevKit.Editor
{
    public static class PackageUtils
    {
        private static readonly Queue<PackageEntry> _queue = new();
        private static bool _isInstalling;

        // Called from menu items with raw git URLs (no PackageEntry overhead needed)
        public static void InstallPackages(params string[] gitUrls)
        {
            var entries = gitUrls.Select(url => new PackageEntry
            {
                packageId = url,
                sourceType = PackageSourceType.Git,
                gitUrl = url,
            }).ToList();
            InstallPackages(entries);
        }

        public static void InstallPackages(IEnumerable<PackageEntry> packages)
        {
            foreach (var p in packages) { _queue.Enqueue(p); }
            if (!_isInstalling) { StartNextInstallation(); }
        }

        private static async void StartNextInstallation()
        {
            _isInstalling = true;
            var entry = _queue.Dequeue();

            if (entry.sourceType == PackageSourceType.ScopedRegistry) { EnsureScopedRegistry(entry); }

            var installId = entry.sourceType == PackageSourceType.Git ? entry.gitUrl : entry.packageId;
            var request = Client.Add(installId);

            while (!request.IsCompleted) { await Task.Delay(100); }

            if (request.Status == StatusCode.Success) { Debug.Log("[GameDevKit] Installed: " + request.Result.packageId); }
            else if (request.Status >= StatusCode.Failure) { Debug.LogError("[GameDevKit] Failed to install " + entry.packageId + ": " + request.Error.message); }

            if (_queue.Count > 0)
            {
                await Task.Delay(100);
                StartNextInstallation();
            }
            else { _isInstalling = false; }
        }

        // Scoped registries live in Packages/manifest.json and must be added before Client.Add()
        private static void EnsureScopedRegistry(PackageEntry entry)
        {
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            var manifest = JsonConvert.DeserializeObject<PackageManifest>(File.ReadAllText(manifestPath));

            if (manifest.scopedRegistries.Any(r => r.url == entry.scopedRegistry.url)) { return; }

            manifest.scopedRegistries.Add(entry.scopedRegistry);
            File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
            Debug.Log($"[GameDevKit] Added scoped registry: {entry.scopedRegistry.name} ({entry.scopedRegistry.url})");
        }

        private class PackageManifest
        {
            public Dictionary<string, string> dependencies { get; set; } = new();
            public List<ScopedRegistry> scopedRegistries { get; set; } = new();
        }
    }
}