using System;
using System.Collections.Generic;

namespace GameDevKit.Installer
{
    [Serializable]
    public class AssemblyDefinitionData
    {
        public string name;
        public string rootNamespace;
        public List<string> references = new();
        public List<string> includePlatforms = new();
        public List<string> excludePlatforms = new();
        public bool allowUnsafeCode;
        public bool overrideReferences;
        public List<string> precompiledReferences = new();
        public bool autoReferenced = true;
        public List<string> defineConstraints = new();
        public List<VersionDefine> versionDefines = new();
        public bool noEngineReferences;
    }

    [Serializable]
    public class VersionDefine
    {
        public string name;
        public string expression;
        public string define;
    }
}