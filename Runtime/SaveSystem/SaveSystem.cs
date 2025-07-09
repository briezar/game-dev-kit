using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;




#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameDevKit.DataPersistence
{
    public class SaveSystem
    {
        public readonly SerializationStrategy Serialization;
        public readonly EncryptionStrategy Encryption;
        public readonly SavePathObfuscationStrategy SavePathObfuscation;
        public readonly WriteStrategy WriteStrategy;

        private readonly string _persistentDataPath;

        public SaveSystem(SerializationStrategy serialization, EncryptionStrategy encryption = null, SavePathObfuscationStrategy savePathObfuscation = null, WriteStrategy writeStrategy = null)
        {
            Serialization = serialization;
            Encryption = encryption ?? new NoEncryptionStrategy();
            SavePathObfuscation = savePathObfuscation ?? new NoObfuscationStrategy();
            WriteStrategy = writeStrategy ?? new File_WriteAllTextStrategy();

            _persistentDataPath = Application.persistentDataPath;
        }


#if UNITY_EDITOR
        [MenuItem("SaveSystem/Test")]
        private static void Test()
        {
            var dataString = "The quick brown fox jumps over the lazy dog";

            var saveSystem1 = new SaveSystem(new JsonUtilitySerializationStrategy());
            var saveSystem2 = new SaveSystem(new JsonUtilitySerializationStrategy(), new XOR_EncryptionStrategy(), new HexObfuscationStrategy(), new File_WriteAllTextStrategy());

            var test1path = "Test/Test1";
            var test2path = "Test/Test2";

            saveSystem1.Save(test1path, dataString);
            Debug.Log($"Test1 path: {test1path}");

            var loadedData = saveSystem1.Load(test1path);
            saveSystem2.Save(test2path, loadedData);
            Debug.Log($"Test2 path: {test2path}");
        }

        [MenuItem("SaveSystem/Open Save Path")]
        private static void OpenSavePath()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }

        [MenuItem("SaveSystem/Decipher")]
        private static void Decipher()
        {
            var path = EditorUtility.OpenFilePanel("Select text file to decipher", Application.persistentDataPath, "");
            if (path.Length != 0)
            {
                var saveSystem = new SaveSystem(new JsonUtilitySerializationStrategy(), new XOR_EncryptionStrategy(), new HexObfuscationStrategy(), new File_WriteAllTextStrategy());
                var dataString = saveSystem.Encryption.Decrypt(File.ReadAllText(path));
                var fileName = saveSystem.SavePathObfuscation.Deobfuscate(path.Split(PathUtils.DirectorySeparators).GetLast());
                Debug.Log($"File name: {fileName}, data:\n{dataString}");
            }
        }
#endif

        public bool HasSave(string filePath)
        {
            var obfuscatedPath = CreateAndGetSavePath(filePath);
            return File.Exists(obfuscatedPath);
        }

        public async UniTask<bool> Save(string filePath, string data, CancellationToken cancellationToken = default)
        {
            var savePath = CreateAndGetSavePath(filePath);

            try
            {
                data = Encryption.Encrypt(data);
                await WriteStrategy.Write(savePath, data, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while saving to {savePath} (original path: {filePath}).\n{ex}");
                return false;
            }
        }

        public UniTask<bool> Save<T>(string filePath, T obj, CancellationToken cancellationToken = default)
        {
            var dataString = Serialization.Serialize(obj);
            return Save(filePath, dataString, cancellationToken);
        }

        public string Load(string filePath)
        {
            var savePath = CreateAndGetSavePath(filePath);

            var dataString = string.Empty;
            try
            {
                dataString = File.ReadAllText(savePath);
                dataString = Encryption.Decrypt(dataString);
            }
            catch (FileNotFoundException)
            {
                Debug.LogWarning($"No save found at [{filePath}]");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while trying to load from [{filePath}].\n{ex}");
            }

            return dataString;
        }

        public T Load<T>(string filePath)
        {
            var dataString = Load(filePath);
            if (dataString.IsNullOrEmpty()) { return default; }

            try
            {
                T result = Serialization.Deserialize<T>(dataString);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while trying to deserialize [{filePath}].\n{ex}");
                // Debug.LogWarning("Invalid JSON format: " + dataString);
                return default;
            }
        }

        public void Delete(string filePath)
        {
            var savePath = CreateAndGetSavePath(filePath);
            try
            {
                File.Delete(savePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Cannot delete save at path {filePath} (original path: {filePath}).\n{ex}");
            }
        }

        private string CreateAndGetSavePath(string filePath)
        {
            var obfuscatedPath = SavePathObfuscation.Obfuscate(filePath);
            obfuscatedPath = Path.Combine(_persistentDataPath, obfuscatedPath);
            Directory.CreateDirectory(Path.GetDirectoryName(obfuscatedPath));
            return obfuscatedPath;
        }
    }
}
