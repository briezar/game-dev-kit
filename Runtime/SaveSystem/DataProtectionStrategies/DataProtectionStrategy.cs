using System;
using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using System.IO;
using System.Text;

namespace GameDevKit.DataPersistence
{
    /// <summary>
    /// File layout: 8 hex digits of the payload hash, "\n", then the payload as UTF-8.
    /// The hash lives in the same file as the data, so a single atomic replace covers both.
    /// </summary>
    public class DataProtectionStrategy
    {
        public const string BackupExtension = ".bak";
        public const string TempExtension = ".tmp";

        private const int HashHexLength = 8;
        private const int HeaderLength = HashHexLength + 1; // hash + '\n'

        public enum VerifyStatus
        {
            Valid,

            /// <summary> The header is missing or malformed. </summary>
            InvalidHeader,

            /// <summary> The payload does not match the hash in the header. </summary>
            HashMismatch,
        }

        /// <summary> Keep the previous save as a backup when a new one replaces it, and use it for recovery. </summary>
        public bool UseBackup { get; init; } = true;

        /// <summary> Skips the hash check and accepts plain files without a header, so saves can be edited by hand. </summary>
        public bool AllowTampering { get; init; } = false;
        public Func<string, uint> FnComputeHash { get; init; } = FastHash.Compute;

        public string GetBackupPath(string savePath) => savePath + BackupExtension;
        public string GetTempPath(string savePath) => savePath + TempExtension;

        public uint ComputeHash(string payload) => FnComputeHash(payload);

        /// <summary>
        /// Splits file text into header and payload and checks the hash.
        /// <br/>The payload is null if the header is invalid, unless <see cref="AllowTampering"/> is true.
        /// </summary>
        public VerifyStatus Unwrap(string fileText, out string payload)
        {
            payload = null;
            if (fileText.Length < HeaderLength
                || fileText[HashHexLength] != '\n'
                || !uint.TryParse(fileText.AsSpan(0, HashHexLength), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var expected))
            {
                if (!AllowTampering) { return VerifyStatus.InvalidHeader; }

                payload = fileText; // accept a plain file without a header
                return VerifyStatus.Valid;
            }

            payload = fileText.Substring(HeaderLength);
            if (AllowTampering || FnComputeHash(payload) == expected) { return VerifyStatus.Valid; }
            return VerifyStatus.HashMismatch;
        }

        /// <summary>
        /// Writes the hash header and the payload to a temp file, then swaps it in.
        /// If <see cref="UseBackup"/> is on, the previous save becomes the backup.
        /// The bytes are encoded into a pooled buffer, so no intermediate string or array is allocated.
        /// </summary>
        public void WriteAtomic(WriteStrategy writeStrategy, string savePath, string payload, uint hash)
        {
            var tempPath = GetTempPath(savePath);
            var byteCount = HeaderLength + Encoding.UTF8.GetByteCount(payload);

            // Rent may return a larger array than asked for, so always pass byteCount along.
            var buffer = ArrayPool<byte>.Shared.Rent(byteCount);
            try
            {
                // Header: the hash as 8 lowercase hex digits, then '\n'.
                Utf8Formatter.TryFormat(hash, buffer, out _, new StandardFormat('x', HashHexLength));
                buffer[HashHexLength] = (byte)'\n';
                Encoding.UTF8.GetBytes(payload, 0, payload.Length, buffer, HeaderLength);
                writeStrategy.Write(tempPath, buffer, byteCount);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            var backupPath = UseBackup ? GetBackupPath(savePath) : null;
            if (File.Exists(savePath)) { ReplaceFile(tempPath, savePath, backupPath); }
            else { File.Move(tempPath, savePath); }
        }

        /// <summary> Deletes the backup and temp files next to the save. Does not delete the save itself. </summary>
        public void DeleteSideFiles(string savePath)
        {
            File.Delete(GetBackupPath(savePath));
            File.Delete(GetTempPath(savePath));
        }

        private static void ReplaceFile(string source, string destination, string backup)
        {
            try
            {
                File.Replace(source, destination, backup);
            }
            catch (Exception ex) when (ex is PlatformNotSupportedException or NotSupportedException or NotImplementedException)
            {
                // Not atomic, but the old save is copied to the backup first.
                if (backup != null) { File.Copy(destination, backup, true); }
                File.Copy(source, destination, true);
                File.Delete(source);
            }
        }
    }
}