using System.IO;

namespace GameDevKit.DataPersistence
{
    public class FileStream_WriteTextStrategy : WriteStrategy
    {
        public readonly int BufferSize;

        /// <summary> Force the data onto the disk before returning, so a power loss right after a save cannot leave an empty file. </summary>
        public readonly bool FlushToDisk;

        public const int DefaultBufferSize = 8192; // 8 KB

        public FileStream_WriteTextStrategy(int bufferSize = DefaultBufferSize, bool flushToDisk = true)
        {
            BufferSize = bufferSize;
            FlushToDisk = flushToDisk;
        }

        public override void Write(string path, byte[] buffer, int length)
        {
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize);
            stream.Write(buffer, 0, length);
            if (FlushToDisk) { stream.Flush(true); }
        }
    }
}