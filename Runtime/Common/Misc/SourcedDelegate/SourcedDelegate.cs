using System;
using System.Collections.Generic;

namespace GameDevKit
{
    public interface ISourcedDelegate
    {
        bool RemoveSource(object source);
    }

    public abstract class SourcedDelegate<TDelegate> : ISourcedDelegate where TDelegate : Delegate
    {
        protected readonly Dictionary<object, TDelegate> _delegates = new();

        private readonly List<TDelegate> _invocationList = new();

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

        public bool RemoveSource(object source) => _delegates.Remove(source);
        public void RemoveAll() => _delegates.Clear();

        protected List<TDelegate> GetInvocationList()
        {
            _invocationList.Clear();
            _invocationList.AddRange(_delegates.Values);
            return _invocationList;
        }
    }

    public class SourcedDelegateBag : List<ISourcedDelegate>
    {
        public void RemoveSource(object source)
        {
            foreach (var del in this)
            {
                del.RemoveSource(source);
            }
        }
    }

}