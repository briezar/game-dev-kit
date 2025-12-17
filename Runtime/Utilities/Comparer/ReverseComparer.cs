using System.Collections.Generic;

namespace GameDevKit.Comparers
{
    public class ReverseComparer<T> : IComparer<T>
    {
        public readonly IComparer<T> Other;
        public ReverseComparer(IComparer<T> other) => Other = other;
        public int Compare(T x, T y) => Other.Compare(y, x);
    }
}
