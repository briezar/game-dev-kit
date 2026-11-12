using System;
using UnityEngine;

namespace GameDevKit
{
    public abstract class ObservableIntBase : ObservableNumber<int, IntChangeInfo>
    {
        protected override bool AreEqual(int a, int b) => a == b;
        protected override int Sum(int a, int b) => a + b;
        protected override IntChangeInfo CreateInfo(int prev, int next) => new(prev, next);

        public static implicit operator int(ObservableIntBase o) => o.Value;
    }

    [Serializable]
    public class ObservableInt : ObservableIntBase
    {
        [SerializeField] private int _value;

        protected override ref int ValueRef => ref _value;

        internal static class EditorProps
        {
            public const string ValueProp = nameof(_value);
        }

        public static explicit operator ObservableInt(int value) => new() { _value = value };
    }

    public class ObservableIntRef : ObservableIntBase
    {
        private readonly RefGetter<int> _getter;
        protected override ref int ValueRef => ref _getter();

        public ObservableIntRef(RefGetter<int> getter) => _getter = getter;
    }
}