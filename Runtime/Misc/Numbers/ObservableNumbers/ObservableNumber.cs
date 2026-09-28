using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    public abstract class ObservableNumber<T> where T : unmanaged
    {
        [SerializeField] protected T _value;

        public T Value => _value;

#if UNITY_EDITOR
        internal static class EditorProps
        {
            public static string ValueProp => nameof(_value);
        }
#endif

        public abstract void Set(T value);
        public virtual void SetWithoutNotify(T value) => _value = value;

        public abstract void Add(T amount);

    }
}