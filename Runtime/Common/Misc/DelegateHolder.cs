
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

    }
}