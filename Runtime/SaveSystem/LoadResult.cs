using System;

namespace GameDevKit.DataPersistence
{
    public enum LoadStatus
    {
        /// <summary> Loaded from the main save file. </summary>
        Ok,

        /// <summary> No save file exists. </summary>
        NotFound,

        /// <summary> The main save was missing or rejected, and a temp or backup copy was loaded instead. </summary>
        RecoveredFromBackup,

        /// <summary> Save files exist, but none could be loaded. </summary>
        Corrupted,
    }

    public readonly struct LoadResult<T>
    {
        public LoadStatus Status { get; }
        public T Value { get; }

        /// <summary> The first problem found while loading, if any. Hash mismatches are <see cref="SaveTamperedException"/>. </summary>
        public Exception Error { get; }
        public bool HasValue => Status is LoadStatus.Ok or LoadStatus.RecoveredFromBackup;

        public LoadResult(LoadStatus status, T value, Exception error)
        {
            Status = status;
            Value = value;
            Error = error;
        }
    }
}