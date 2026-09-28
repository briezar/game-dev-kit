using System;

namespace GameDevKit
{
    public class SourcedAction : SourcedDelegate<Action>
    {
        public void InvokeSource(object source) => this[source]?.Invoke();

        public void Invoke()
        {
            using var pooled = GetInvocationList(out var list);
            foreach (var action in list) { action?.Invoke(); }
        }
    }

    public class SourcedAction<T> : SourcedDelegate<Action<T>>
    {
        public void InvokeSource(object source, T value) => this[source]?.Invoke(value);

        public void Invoke(T arg)
        {
            using var pooled = GetInvocationList(out var list);
            foreach (var action in list) { action?.Invoke(arg); }
        }
    }

    public class SourcedAction<T1, T2> : SourcedDelegate<Action<T1, T2>>
    {
        public void InvokeSource(object source, T1 arg1, T2 arg2) => this[source]?.Invoke(arg1, arg2);

        public void Invoke(T1 arg1, T2 arg2)
        {
            using var pooled = GetInvocationList(out var list);
            foreach (var action in list) { action?.Invoke(arg1, arg2); }
        }
    }
}