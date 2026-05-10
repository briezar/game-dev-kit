using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    public abstract class ObservableNumber<T> where T : unmanaged
    {
        [SerializeField] protected T _value;

        public abstract T Value { get; set; }

        public virtual void SetValueWithoutNotify(T value) => _value = value;
    }
}