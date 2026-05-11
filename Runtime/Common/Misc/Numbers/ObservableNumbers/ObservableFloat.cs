using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public class ObservableFloat : ObservableNumber<float>
    {
        public readonly SourcedAction<FloatChangeInfo> OnValueChanged = new();

        public override float Value
        {
            get => _value;
            set
            {
                var prev = _value;
                if (Mathf.Approximately(value, prev)) { return; }

                _value = value;
                OnValueChanged?.Invoke(new(prev, value));
            }
        }

        public static implicit operator ObservableFloat(float value) => new() { _value = value };
        public static implicit operator float(ObservableFloat value) => value.Value;

    }
}