using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit.Installer
{
    public enum PackageSourceType { Git, ScopedRegistry }

    [Serializable]
    public class PackageEntry
    {
        [Tooltip("The UPM package id, e.g. com.cysharp.unitask")]
        public string packageId;

        public PackageSourceType sourceType;

        [Tooltip("Git URL (used when sourceType is Git)")]
        public string gitUrl;

        [Tooltip("Scoped Registry (used when sourceType is ScopedRegistry)")]
        public ScopedRegistry scopedRegistry;
    }

    [Serializable]
    public class ScopedRegistry
    {
        [Tooltip("Registry name, e.g. 'package.openupm.com' (used when sourceType is ScopedRegistry)")]
        public string name;

        [Tooltip("Registry URL, e.g. 'https://package.openupm.com' (used when sourceType is ScopedRegistry)")]
        public string url;

        [Tooltip("Scopes for the registry, e.g. 'com.cysharp' (used when sourceType is ScopedRegistry)")]
        public string[] scopes; // Do not remove! Required for json parsing.
    }

    public class PackageManifest
    {
        public Dictionary<string, string> dependencies = new();
        public List<ScopedRegistry> scopedRegistries = new();
    }
}
