using System.Text;
using UnityEngine.Pool;

namespace GameDevKit.Pool
{
    public static class StringBuilderPool
    {
        private static readonly ObjectPool<StringBuilder> _pool = new(() => new StringBuilder(), sb => sb.Clear());

        public static PooledObject<StringBuilder> Get(out StringBuilder value) => _pool.Get(out value);
        public static StringBuilder Get() => _pool.Get();
        public static void Release(StringBuilder sb) => _pool.Release(sb);
    }
}