using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameDevKit.DataPersistence
{
    public class SaveSystem
    {
        public SerializationStrategy Serialization { get; init; } = new JsonUtilitySerializationStrategy();
        public WriteStrategy WriteStrategy { get; init; } = new FileStream_WriteTextStrategy();
        public DataProtectionStrategy DataProtection { get; init; } = new DataProtectionStrategy();

        /// <summary> Write on a thread-pool thread in <see cref="Save(string, string, CancellationToken)"/>. Has no effect on WebGL, which has no threads. </summary>
        public bool UseThreadPool { get; init; } = true;

        /// <summary> Skip the write when the content hash equals the last successful write to the same path. </summary>
        public bool SkipUnchangedWrites { get; init; } = true;

        private const bool IsThreadPoolSupported =
#if UNITY_WEBGL && !UNITY_EDITOR
            false;
#else
            true;
#endif

        /// <summary> Per-file state. Writes to one file hold its lock, so they never overlap. </summary>
        private sealed class PathState
        {
            public int LatestVersion;  // bumped by every save call
            public int WrittenVersion; // newest version on disk, guarded by the lock
            public uint? LastHash;     // hash of the last successful write, guarded by the lock
        }

        private readonly string _persistentDataPath;
        private readonly ConcurrentDictionary<string, PathState> _pathStates = new();

        public SaveSystem() => _persistentDataPath = Application.persistentDataPath;


#if UNITY_EDITOR
        [MenuItem(EditorConstants.MenuItemPath + "SaveSystem/Open Save Path")]
        private static void OpenSavePath() => EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
#endif

        /// <summary> True if a save exists, including one that can only be recovered from a temp or backup file. </summary>
        public bool HasSave(string filePath)
        {
            var savePath = GetSavePath(filePath);
            if (File.Exists(savePath) || File.Exists(DataProtection.GetTempPath(savePath))) { return true; }
            return DataProtection.UseBackup && File.Exists(DataProtection.GetBackupPath(savePath));
        }

        /// <summary>
        /// Saves in the background. If saves overlap, the newest call wins: a save is skipped (and reports success)
        /// when a newer one is already on disk.
        /// </summary>
        public async UniTask<bool> Save(string filePath, string data, CancellationToken cancellationToken = default)
        {
            var savePath = GetSavePath(filePath);
            var state = GetState(savePath);
            var version = Interlocked.Increment(ref state.LatestVersion);

            try
            {
                if (UseThreadPool && IsThreadPoolSupported) { await UniTask.SwitchToThreadPool(); }
                cancellationToken.ThrowIfCancellationRequested();
                return WriteToDisk(state, savePath, data, version);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            finally
            {
                await UniTask.SwitchToMainThread();
            }
        }

        public UniTask<bool> Save<T>(string filePath, T obj, CancellationToken cancellationToken = default) =>
            TrySerialize(filePath, obj, out var data) ? Save(filePath, data, cancellationToken) : UniTask.FromResult(false);

        /// <summary>
        /// Saves on the calling thread and blocks until the file is written. Use it where you cannot wait,
        /// such as on quit or pause. Waits for a background write to the same file that is already running.
        /// </summary>
        public bool SaveImmediately(string filePath, string data)
        {
            var savePath = GetSavePath(filePath);
            var state = GetState(savePath);
            var version = Interlocked.Increment(ref state.LatestVersion);
            return WriteToDisk(state, savePath, data, version);
        }

        public bool SaveImmediately<T>(string filePath, T obj) => TrySerialize(filePath, obj, out var data) && SaveImmediately(filePath, data);

        private bool TrySerialize<T>(string filePath, T obj, out string data)
        {
            try
            {
                data = Serialization.Serialize(obj);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while serializing [{filePath}].\n{ex}");
                data = null;
                return false;
            }
        }

        private bool WriteToDisk(PathState state, string savePath, string data, int version)
        {
            lock (state)
            {
                if (version < state.WrittenVersion) { return true; } // a newer save is already on disk

                try
                {
                    var hash = DataProtection.ComputeHash(data);
                    if (SkipUnchangedWrites && state.LastHash == hash && File.Exists(savePath))
                    {
                        state.WrittenVersion = version;
                        return true;
                    }

                    var directory = Path.GetDirectoryName(savePath);
                    if (!string.IsNullOrEmpty(directory)) { Directory.CreateDirectory(directory); }

                    DataProtection.WriteAtomic(WriteStrategy, savePath, data, hash);
                    state.LastHash = hash;
                    state.WrittenVersion = version;
                    return true;
                }
                catch (Exception ex)
                {
                    state.LastHash = null;
                    Debug.LogError($"Error while saving to {savePath}.\n{ex}");
                    return false;
                }
            }
        }

        /// <summary> Returns the text, or null if nothing could be loaded. </summary>
        public string Load(string filePath) => LoadWithStatus(filePath).Value;

        /// <summary> Returns the deserialized value, or default if nothing could be loaded. </summary>
        public T Load<T>(string filePath) => LoadWithStatus<T>(filePath).Value;

        /// <summary> Like <see cref="Load(string)"/>, but tells you whether the save was missing, recovered or corrupted. </summary>
        public LoadResult<string> LoadWithStatus(string filePath) => LoadWithStatus(filePath, static text => text);

        /// <summary> Like <see cref="Load{T}(string)"/>, but tells you whether the save was missing, recovered or corrupted. </summary>
        public LoadResult<T> LoadWithStatus<T>(string filePath) => LoadWithStatus(filePath, Serialization.Deserialize<T>);

        /// <summary>
        /// Loads the newest valid file among the save, its temp file and its backup. A file is only used if its hash
        /// matches and <paramref name="parse"/> succeeds on it.
        /// </summary>
        public LoadResult<T> LoadWithStatus<T>(string filePath, Func<string, T> parse)
        {
            var savePath = GetSavePath(filePath);

            // Newest first. A temp file only passes verification if it was written completely.
            var candidateCount = DataProtection.UseBackup ? 3 : 2;

            Exception firstError = null;
            var anyFileFound = false;
            for (var i = 0; i < candidateCount; i++)
            {
                var path = i switch
                {
                    0 => savePath,
                    1 => DataProtection.GetTempPath(savePath),
                    _ => DataProtection.GetBackupPath(savePath),
                };
                if (!File.Exists(path)) { continue; }

                anyFileFound = true;
                if (TryLoadFile(path, parse, out var value, out var error))
                {
                    if (i == 0) { return new LoadResult<T>(LoadStatus.Ok, value, null); }

                    Debug.LogWarning($"Save at [{savePath}] is missing or invalid, recovered from [{path}].");
                    // Overwrite a bad main save with the good copy, so the next save rotates a good file into the backup.
                    // Skip it if the main save only failed to read (for example a locked file): it may be fine.
                    if (firstError is not (IOException or UnauthorizedAccessException)) { RestoreMain(path, savePath); }
                    return new LoadResult<T>(LoadStatus.RecoveredFromBackup, value, firstError);
                }

                Debug.LogError($"Cannot load save from [{path}].\n{error}");
                firstError ??= error;
            }

            if (!anyFileFound) { Debug.LogWarning($"No save found at [{savePath}]"); }
            return new LoadResult<T>(anyFileFound ? LoadStatus.Corrupted : LoadStatus.NotFound, default, firstError);
        }

        private bool TryLoadFile<T>(string path, Func<string, T> parse, out T value, out Exception error)
        {
            value = default;
            error = null;
            try
            {
                var fileText = File.ReadAllText(path);
                if (fileText.Length == 0)
                {
                    error = new InvalidDataException("Save file is empty.");
                    return false;
                }

                // Verify before deserializing.
                var status = DataProtection.Unwrap(fileText, out var payload);
                if (status is not DataProtectionStrategy.VerifyStatus.Valid)
                {
                    error = status is DataProtectionStrategy.VerifyStatus.HashMismatch
                        ? new SaveTamperedException($"Hash mismatch for [{path}].")
                        : new InvalidDataException($"Missing or malformed header in [{path}].");
                    return false;
                }

                value = parse(payload);
                return true;
            }
            catch (Exception ex)
            {
                error = ex;
                return false;
            }
        }

        private static void RestoreMain(string sourcePath, string savePath)
        {
            try
            {
                File.Copy(sourcePath, savePath, true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Cannot restore [{savePath}] from [{sourcePath}].\n{ex}");
            }
        }

        /// <summary> Deletes the save and its side files. Saves requested before this call are dropped. </summary>
        public void Delete(string filePath)
        {
            var savePath = GetSavePath(filePath);
            var state = GetState(savePath);
            lock (state)
            {
                state.WrittenVersion = Volatile.Read(ref state.LatestVersion);
                state.LastHash = null;

                try
                {
                    File.Delete(savePath);
                    DataProtection.DeleteSideFiles(savePath);
                }
                catch (DirectoryNotFoundException) { }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Cannot delete save at path {savePath} (original path: {filePath}).\n{ex}");
                }
            }
        }

        private PathState GetState(string savePath) => _pathStates.GetOrAdd(savePath, static _ => new PathState());

        private string GetSavePath(string filePath) => Path.Combine(_persistentDataPath, filePath);
    }
}