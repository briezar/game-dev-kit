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
        public static async void InstallPackages(IEnumerable<PackageEntry> packageEntries)
        {
            EnsureScopedRegistry(packageEntries);

            var packagesToAdd = packageEntries.Select(p => p.sourceType is PackageSourceType.Git ? p.gitUrl : p.packageId).ToList();

            var request = Client.AddAndRemove(packagesToAdd.ToArray());
            var timer = new Timer(packagesToAdd.Count * 10);
            while (!request.IsCompleted)
            {
                EditorUtility.DisplayProgressBar(
                    "GameDevKit — Checking Packages",
                    "Installing packages...",
                    timer.GetProgressMax(0.9f));
                await Task.Delay(timer.IntervalMs);
                timer.Tick();
            }

            EditorUtility.ClearProgressBar();

            if (request.Status >= StatusCode.Failure)
            {
                Debug.LogError($"[GameDevKit] Failed to install: {request.Error.message}");
                return;
            }

            if (request.Result == null)
            {
                await Task.Yield();
                if (request.Result == null)
                {
                    Debug.LogWarning($"request.Result is null!");
                    return;
                }
            }

            if (request.Status == StatusCode.Success)
            {
                var installedPackages = request.Result.Where(pInfo => packageEntries.Any(pEntry => string.Equals(pInfo.name, pEntry.packageId, StringComparison.Ordinal))).ToList();
                Debug.Log($"[GameDevKit] Installed {installedPackages.Count} package(s):\n{string.Join("\n", installedPackages.Select(p => p.packageId))}");
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

    }

    internal struct Timer
    {
        public float Duration;
        public float Interval;

        private float _elapsed;
        public float Elapsed
        {
            readonly get => _elapsed;
            set => _elapsed = Mathf.Clamp(value, 0, Duration);
        }
        public readonly float Progress => Duration > 0f ? Mathf.Clamp01(Elapsed / Duration) : 1f;
        public readonly int IntervalMs => (int)(Interval * 1000);

        public Timer(float duration, float interval = 0.1f)
        {
            (Duration, Interval) = (duration, interval);
            _elapsed = 0;
        }

        public readonly float GetProgressMax(float max) => Mathf.Min(Progress, max);

        public void Tick(float? delta = null) => Elapsed += delta ?? Interval;
    }
}