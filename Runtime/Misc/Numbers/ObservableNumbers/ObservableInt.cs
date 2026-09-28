using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public class ObservableInt : ObservableNumber<int>
    {
        public readonly SourcedAction<IntChangeInfo> OnValueChanged = new();

        public override void Set(int value)
        {
            var prev = _value;
            if (value == prev) { return; }

            _value = value;
            OnValueChanged?.Invoke(new(prev, value));
        }

        public override void Add(int amount) => Set(Value + amount);

        public static explicit operator ObservableInt(int value) => new() { _value = value };
        public static implicit operator int(ObservableInt value) => value.Value;

    }
}