using System;
using System.Buffers;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameDevKit.DataPersistence
{
    public class DataProtectionStrategy
    {
        public const string BackupExtension = ".bak";

        public bool UseBackup { get; init; } = true;
        public bool AllowTampering { get; init; } = false;
        public Func<string, uint> FnComputeHash { get; init; } = FastHash.Compute;

        public string GetBackupPath(string savePath) => savePath + BackupExtension;

        public async UniTask WriteWithBackup(WriteStrategy writeStrategy, string savePath, string data, CancellationToken cancellationToken = default)
        {
            var backupPath = GetBackupPath(savePath);
            await writeStrategy.Write(backupPath, data, cancellationToken);

            // save is not corrupted, move it to the correct location
            if (File.Exists(savePath))
            {
                File.Replace(backupPath, savePath, null);
            }
            else
            {
                File.Move(backupPath, savePath);
            }

            WriteHash(savePath, data);
        }

        public void WriteHash(string savePath, string data)
        {
            uint hash = FnComputeHash(data);
            var hashBuffer = ArrayPool<byte>.Shared.Rent(4);
            hashBuffer[0] = (byte)(hash >> 0);
            hashBuffer[1] = (byte)(hash >> 8);
            hashBuffer[2] = (byte)(hash >> 16);
            hashBuffer[3] = (byte)(hash >> 24);

            string hashPath = savePath + ".hash";
            using var fs = new FileStream(hashPath, FileMode.Create, FileAccess.Write, FileShare.None, 4, useAsync: false);
            fs.Write(hashBuffer, 0, 4);
            ArrayPool<byte>.Shared.Return(hashBuffer);
        }

        public uint ReadHash(string savePath)
        {
            string hashPath = savePath + ".hash";
            if (!File.Exists(hashPath)) { return 0; }

            using var fs = new FileStream(hashPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4, useAsync: false);
            var hashBuffer = ArrayPool<byte>.Shared.Rent(4);
            if (fs.Read(hashBuffer, 0, 4) != 4) { ArrayPool<byte>.Shared.Return(hashBuffer); return 0; }

            uint result = (uint)(hashBuffer[0]
                | (hashBuffer[1] << 8)
                | (hashBuffer[2] << 16)
                | (hashBuffer[3] << 24));

            ArrayPool<byte>.Shared.Return(hashBuffer);
            return result;
        }

        public bool IsValidData(string savePath, string data)
        {
            uint actualHash = ReadHash(savePath);
            var expectedHash = FnComputeHash(data);
            return actualHash == expectedHash;
        }
    }
}