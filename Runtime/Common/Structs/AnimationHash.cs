using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    /// <summary> Wrapper for better context </summary>
    public struct AnimationHash
    {
        public int value;

        public AnimationHash(string paramName)
        {
            value = Animator.StringToHash(paramName);
        }

        public static implicit operator int(AnimationHash hash) => hash.value;

        public static implicit operator AnimationHash(int intValue) => new() { value = intValue };
        public static implicit operator AnimationHash(string paramName) => new(paramName);
    }
}
