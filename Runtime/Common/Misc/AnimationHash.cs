using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    /// <summary> Animator.StringToHash wrapper for better context </summary>
    public struct AnimationHash
    {
        public int value;

        public AnimationHash(string paramName) => value = Animator.StringToHash(paramName);

        public static implicit operator int(AnimationHash hash) => hash.value;

        public static implicit operator AnimationHash(int intValue) => new() { value = intValue };
        public static implicit operator AnimationHash(string paramName) => new(paramName);
    }

    [Serializable]
    public struct SerializableAnimationHash
    {
        [SerializeField] private string _paramName;

        private int? _value;
        public int Value => _value ??= Animator.StringToHash(_paramName);

#if UNITY_EDITOR
        internal static class EditorProps
        {
            public static string ParamName => nameof(_paramName);
        }
#endif

        public SerializableAnimationHash(string paramName)
        {
            _paramName = paramName;
            _value = Animator.StringToHash(paramName);
        }

        public static implicit operator int(SerializableAnimationHash hash) => hash.Value;
        public static implicit operator SerializableAnimationHash(string paramName) => new(paramName);
    }
}
