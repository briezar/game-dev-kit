using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    public class ChainComparer<T> : IComparer<T>
    {
        private readonly IComparer<T> _current, _next;

        public ChainComparer(IComparer<T> current, IComparer<T> next)
        {
            _current = current;
            _next = next;
        }
        
        public int Compare(T x, T y)
        {
            int result = _current.Compare(x, y);
            if (result == 0)
            {
                result = _next.Compare(x, y);
            }
            return result;
        }
    }
}
