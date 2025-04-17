using System.Collections.Generic;

namespace GameDevKit
{
    public static class ComparerExtension
    {
        public static IComparer<T> Then<T>(this IComparer<T> current, IComparer<T> next) => new ChainComparer<T>(current, next);
        public static IComparer<T> Reverse<T>(this IComparer<T> comparer) => comparer is ReverseComparer<T> reverse ? reverse.Other : new ReverseComparer<T>(comparer);
    }
}
