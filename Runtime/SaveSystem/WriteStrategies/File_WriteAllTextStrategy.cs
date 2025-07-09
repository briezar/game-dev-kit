using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameDevKit.DataPersistence
{
    public class File_WriteAllTextStrategy : WriteStrategy
    {
        public readonly bool UseAsync;

        public File_WriteAllTextStrategy(bool useAsync = false)
        {
            UseAsync = useAsync;
        }

        public override async UniTask Write(string path, string content, CancellationToken cancellationToken = default)
        {
            if (UseAsync)
            {
                await File.WriteAllTextAsync(path, content, cancellationToken);
            }
            else
            {
                File.WriteAllText(path, content);
            }
        }
    }
}