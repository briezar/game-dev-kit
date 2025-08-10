using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameDevKit.DataPersistence
{
    public class SaveSystem
    {
        public SerializationStrategy Serialization { get; init; } = new JsonUtilitySerializationStrategy();
        public EncryptionStrategy Encryption { get; init; } = new NoEncryptionStrategy();
        public SavePathObfuscationStrategy SavePathObfuscation { get; init; } = new NoObfuscationStrategy();
        public WriteStrategy WriteStrategy { get; init; } = new File_WriteAllTextStrategy();
        public DataProtectionStrategy DataProtection { get; init; } = new DataProtectionStrategy();

        private readonly string _persistentDataPath;

        public SaveSystem()
        {
            _persistentDataPath = Application.persistentDataPath;
        }


#if UNITY_EDITOR
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
                var saveSystem = new SaveSystem() { Encryption = new XOR_EncryptionStrategy(), SavePathObfuscation = new HexObfuscationStrategy() };
                var data = saveSystem.Encryption.Decrypt(File.ReadAllText(path));
                var fileName = saveSystem.SavePathObfuscation.Deobfuscate(path.Split(PathUtils.DirectorySeparators).GetLast());
                Debug.Log($"File name: {fileName}, data:\n{data}");
            }
        }
#endif

        public bool HasSave(string filePath) => File.Exists(GetSavePath(filePath, false));

        private async UniTask<bool> InternalSave(string filePath, string data, CancellationToken cancellationToken = default)
        {
            var savePath = GetSavePath(filePath);

            try
            {
                data = Encryption.Encrypt(data);

                if (!DataProtection.UseBackup)
                {
                    await WriteStrategy.Write(savePath, data, cancellationToken);
                }
                else
                {
                    await DataProtection.WriteWithBackup(WriteStrategy, savePath, data, cancellationToken);
                }


                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while saving to {savePath} (original path: {filePath}).\n{ex}");
                return false;
            }
        }

        public UniTask<bool> Save(string filePath, string data, CancellationToken cancellationToken = default)
        {
            return InternalSave(filePath, data, cancellationToken);
        }

        public UniTask<bool> Save<T>(string filePath, T obj, CancellationToken cancellationToken = default)
        {
            var data = Serialization.Serialize(obj);
            return InternalSave(filePath, data, cancellationToken);
        }

        private string InternalLoad(string savePath)
        {
            string data;
            try
            {
                data = File.ReadAllText(savePath);
                data = Encryption.Decrypt(data);
            }
            catch (FileNotFoundException)
            {
                Debug.LogWarning($"No save found at [{savePath}]");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while trying to load from [{savePath}].\n{ex}");
                throw;
            }

            return data;
        }

        public string Load(string filePath)
        {
            var savePath = GetSavePath(filePath, false);
            return InternalLoad(savePath);
        }

        public T Load<T>(string filePath)
        {
            var savePath = GetSavePath(filePath, false);
            T result;

            try
            {
                var data = InternalLoad(savePath);
                result = Serialization.Deserialize<T>(data);

                if (!DataProtection.AllowTampering)
                {
                    var isTampered = !DataProtection.IsValidData(savePath, data);
                    if (isTampered)
                    {
                        throw new SaveTamperedException();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while trying to deserialize [{filePath}].\n{ex}");

                if (DataProtection.UseBackup)
                {
                    var backupPath = DataProtection.GetBackupPath(savePath);
                    Debug.LogWarning($"Trying to load backup save from [{backupPath}]");
                    var data = InternalLoad(backupPath);
                    result = Serialization.Deserialize<T>(data);
                }
                else
                {
                    result = default;
                }
            }
            return result;
        }

        public void Delete(string filePath)
        {
            var savePath = GetSavePath(filePath);
            try
            {
                File.Delete(savePath);
                if (DataProtection.UseBackup)
                {
                    File.Delete(DataProtection.GetBackupPath(savePath));
                }
            }
            catch (DirectoryNotFoundException) { }
            catch (Exception ex)
            {
                Debug.LogWarning($"Cannot delete save at path {savePath} (original path: {filePath}).\n{ex}");
            }
        }

        private string GetSavePath(string filePath, bool createDirectory = true)
        {
            var obfuscatedPath = SavePathObfuscation.Obfuscate(filePath);
            var savePath = Path.Combine(_persistentDataPath, obfuscatedPath);
            if (createDirectory)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }
            return savePath;
        }
    }
}
