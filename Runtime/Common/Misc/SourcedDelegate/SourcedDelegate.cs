using System;
using System.Collections.Generic;

namespace GameDevKit
{
    public class SourcedDelegate<TDelegate> where TDelegate : Delegate
    {
        protected readonly Dictionary<object, TDelegate> _delegates = new();

        public TDelegate this[object source]
        {
            get
            {
                _delegates.TryAdd(source, default);
                return _delegates[source];
            }
            set
            {
                _delegates.TryAdd(source, default);
                _delegates[source] = value;
            }
        }

        public bool Unsubscribe(object source) => _delegates.Remove(source);
        public void Clear() => _delegates.Clear();
    }
}