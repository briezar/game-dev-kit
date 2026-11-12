using System;
using UnityEngine;

namespace GameDevKit
{
    public abstract class ObservableFloatBase : ObservableNumber<float, FloatChangeInfo>
    {
        protected override bool AreEqual(float a, float b) => Mathf.Approximately(a, b);
        protected override float Sum(float a, float b) => a + b;
        protected override FloatChangeInfo CreateInfo(float prev, float next) => new(prev, next);

        public static implicit operator float(ObservableFloatBase o) => o.Value;
    }

    [Serializable]
    public class ObservableFloat : ObservableFloatBase
    {
        [SerializeField] private float _value;

        internal static class EditorProps
        {
            public const string ValueProp = nameof(_value);
        }

        protected override ref float ValueRef => ref _value;

        public static explicit operator ObservableFloat(float value) => new() { _value = value };
    }

    public class ObservableFloatRef : ObservableFloatBase
    {
        private readonly RefGetter<float> _getter;
        protected override ref float ValueRef => ref _getter();

        public ObservableFloatRef(RefGetter<float> getter) => _getter = getter;
    }
}