using System;

namespace GameDevKit
{
    public class SourcedAction : SourcedDelegate<Action>
    {
        public void InvokeSource(object source)
        {
            if (_delegates.TryGetValue(source, out var action)) { action?.Invoke(); }
        }

        public void Invoke()
        {
            using var pooled = GetInvocationList(out var list);
            foreach (var action in list) { action?.Invoke(); }
        }
    }

    public class SourcedAction<T> : SourcedDelegate<Action<T>>
    {
        public T LatestValue { get; private set; }

        public void InvokeLatest(object source)
        {
            if (_delegates.TryGetValue(source, out var action)) { action?.Invoke(LatestValue); }
        }

        public void Invoke(T arg)
        {
            LatestValue = arg;
            using var pooled = GetInvocationList(out var list);
            foreach (var action in list) { action?.Invoke(arg); }
        }
    }

    public class SourcedAction<T1, T2> : SourcedDelegate<Action<T1, T2>>
    {
        public (T1 arg1, T2 arg2) LatestValue { get; private set; }

        public void InvokeLatest(object source)
        {
            if (_delegates.TryGetValue(source, out var action))
            {
                action?.Invoke(LatestValue.arg1, LatestValue.arg2);
            }
        }

        public void Invoke(T1 arg1, T2 arg2)
        {
            LatestValue = (arg1, arg2);
            using var pooled = GetInvocationList(out var list);
            foreach (var action in list) { action?.Invoke(arg1, arg2); }
        }
    }
}