
using System;
using System.Collections.Generic;

namespace GameDevKit
{
    public class DelegateHolder<TSource, TDelegate> where TDelegate : Delegate
    {
        private readonly Dictionary<TSource, TDelegate> _delegates = new();

        public TDelegate this[TSource source]
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

        public TDelegate Set(TSource source, TDelegate del)
        {
            _delegates[source] = del;
            return del;
        }

        public TDelegate Unset(TSource source)
        {
            if (_delegates.TryGetValue(source, out var del))
            {
                _delegates.Remove(source);
                return del;
            }
            return null;
        }

    }
}