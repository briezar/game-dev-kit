using System;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEngine;

namespace GameDevKit
{
    public static class AllocCounter
    {
        private const string BytesName = "GC Allocated In Frame";
        private const string CountName = "GC Allocation In Frame Count";

        public static Result Measure(Action action)
        {
            var tracker = Tracker.Start();

            try
            {
                action();
                return tracker.Stop();
            }
            finally
            {
                tracker.Dispose();
            }
        }

        public static Scope MeasureScope(string name = null) => new(name);

        /// <summary>
        /// Logs the result.
        /// The first call to a method allocates while it is being JIT-compiled, so this is kept out of Dispose to keep that allocation outside the measurement.
        /// NoInlining stops the JIT from merging it back into Dispose.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Report(string name, Result result) => Debug.Log($"GC.Alloc in {name ?? "scope"}: {result}");

        public readonly struct Result
        {
            public readonly long Bytes;
            public readonly int Count;

            public Result(long bytes, int count) => (Bytes, Count) = (bytes, count);

            public override string ToString() => $"{Bytes} B in {Count} allocations";
        }

        public struct Scope : IDisposable
        {
            private Tracker _tracker;
            private readonly string _name;

            public Scope(string name)
            {
                _name = name;
                _tracker = Tracker.Start();
            }

            public void Dispose()
            {
                if (!_tracker.Valid) { return; }

                var result = _tracker.Stop();

                Report(_name, result);
            }
        }

        private struct Tracker : IDisposable
        {
            private ProfilerRecorder _bytes;
            private ProfilerRecorder _count;
            private long _startBytes;
            private long _startCount;

            public bool Valid => _bytes.Valid && _count.Valid;

            public static Tracker Start()
            {
                var tracker = new Tracker
                {
                    _bytes = ProfilerRecorder.StartNew(ProfilerCategory.Memory, BytesName),
                    _count = ProfilerRecorder.StartNew(ProfilerCategory.Memory, CountName)
                };

                if (!tracker.Valid)
                {
                    Debug.LogWarning($"AllocCounter: could not start '{BytesName}' / '{CountName}'.");
                    tracker.Dispose();
                    return tracker;
                }

                tracker._startBytes = tracker._bytes.CurrentValue;
                tracker._startCount = tracker._count.CurrentValue;

                return tracker;
            }

            public Result Stop()
            {
                if (!Valid) { return default; }

                var result = new Result(
                    _bytes.CurrentValue - _startBytes,
                    (int)(_count.CurrentValue - _startCount));

                Dispose();

                return result;
            }

            public void Dispose()
            {
                if (_bytes.Valid)
                {
                    _bytes.Dispose();
                }

                if (_count.Valid)
                {
                    _count.Dispose();
                }

                _bytes = default;
                _count = default;
            }
        }
    }
}