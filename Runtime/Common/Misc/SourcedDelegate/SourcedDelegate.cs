using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace GameDevKit
{
    public interface ISourcedDelegate
    {
        bool UnsubscribeSource(object source);
    }

    public abstract class SourcedDelegate<TDelegate> : ISourcedDelegate where TDelegate : Delegate
    {
        protected readonly Dictionary<object, TDelegate> _delegates = new();

        public TDelegate this[object source]
        {
            get
            {
                _delegates.TryAdd(source, default);
                return _delegates[source];
            }

            set => _delegates[source] = value;
        }

        public bool UnsubscribeSource(object source) => _delegates.Remove(source);
        public void UnsubscribeAll() => _delegates.Clear();

        protected PooledObject<List<TDelegate>> GetInvocationList(out List<TDelegate> list)
        {
            var pooledObj = ListPool<TDelegate>.Get(out list);
            list.AddRange(_delegates.Values);
            return pooledObj;
        }
    }

    public class SourcedDelegateBag : List<ISourcedDelegate>
    {
        public void RemoveSource(object source)
        {
            foreach (var del in this)
            {
                del.UnsubscribeSource(source);
            }
        }
    }

}