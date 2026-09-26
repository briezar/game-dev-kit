using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public struct AnimationHash
    {
#if UNITY_EDITOR
        [SerializeField] private string _paramName;
#endif

        [SerializeField] private int _value;

        public readonly int Value => _value;

#if UNITY_EDITOR
        internal static class EditorProps
        {
            public static string ParamName => nameof(_paramName);
            public static string Value => nameof(_value);
        }
#endif

        public AnimationHash(string paramName)
        {
#if UNITY_EDITOR
            _paramName = paramName;
#endif
            _value = Animator.StringToHash(paramName);
        }

        public static implicit operator int(AnimationHash hash) => hash.Value;
        public static implicit operator AnimationHash(string paramName) => new(paramName);
        public static implicit operator AnimationHash(int intValue) => new() { _value = intValue };
    }
}
