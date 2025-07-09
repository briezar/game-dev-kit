using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameDevKit.DataPersistence
{
    public abstract class WriteStrategy
    {
        public abstract UniTask Write(string path, string content, CancellationToken cancellationToken = default);
    }
}