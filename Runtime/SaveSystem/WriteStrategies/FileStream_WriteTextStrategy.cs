using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace GameDevKit.DataPersistence
{
    public class FileStream_WriteTextStrategy : WriteStrategy
    {
        public readonly int BufferSize;
        public readonly bool UseAsync;

        public const int DefaultBufferSize = 8192; // 8 KB

        public FileStream_WriteTextStrategy(int bufferSize = DefaultBufferSize, bool useAsync = true)
        {
            BufferSize = bufferSize;
            UseAsync = useAsync;
        }

        public override async UniTask Write(string path, string content, CancellationToken cancellationToken = default)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, UseAsync);
            await stream.WriteAsync(bytes, cancellationToken);
        }
    }
}