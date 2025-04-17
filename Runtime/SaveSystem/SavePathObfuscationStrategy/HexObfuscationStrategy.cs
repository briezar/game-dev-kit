using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace GameDevKit.DataPersistence
{
    public class HexObfuscationStrategy : SavePathObfuscationStrategy
    {
        public readonly Encoding Encoding;

        public HexObfuscationStrategy(Encoding encoding = null)
        {
            Encoding = encoding ?? Encoding.UTF8;
        }

        public override string Obfuscate(string filePath)
        {
            var paths = filePath.Split(PathUtils.DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = paths[i].ToHex(Encoding);
            }
            return Path.Combine(paths);
        }

        public override string Deobfuscate(string filePath)
        {
            var paths = filePath.Split(PathUtils.DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < paths.Length; i++)
            {
                try
                {
                    paths[i] = paths[i].FromHex(Encoding);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unable to parse hex from {paths[i]}.\n{ex}");
                }
            }
            return Path.Combine(paths);
        }
    }
}