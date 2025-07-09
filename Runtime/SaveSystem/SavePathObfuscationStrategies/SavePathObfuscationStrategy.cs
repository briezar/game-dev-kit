using UnityEngine;

namespace GameDevKit.DataPersistence
{
    public abstract class SavePathObfuscationStrategy
    {
        public abstract string Obfuscate(string filePath);
        public abstract string Deobfuscate(string filePath);
    }
}