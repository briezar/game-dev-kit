using System;

namespace GameDevKit
{
    public class SourcedAction : SourcedDelegate<Action>
    {
        public void Invoke()
        {
            foreach (var action in GetInvocationList()) { action?.Invoke(); }
        }
    }

    public class SourcedAction<T> : SourcedDelegate<Action<T>>
    {
        public void Invoke(T arg)
        {
            foreach (var action in GetInvocationList()) { action?.Invoke(arg); }
        }
    }

    public class SourcedAction<T1, T2> : SourcedDelegate<Action<T1, T2>>
    {
        public void Invoke(T1 arg1, T2 arg2)
        {
            foreach (var action in GetInvocationList()) { action?.Invoke(arg1, arg2); }
        }
    }
}