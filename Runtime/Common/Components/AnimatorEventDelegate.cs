using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit
{
    public class AnimatorEventDelegate : MonoBehaviour
    {
        public UnityEvent OnAnimationEventFired;

        public void AnimationEventFired()
        {
            OnAnimationEventFired?.Invoke();
        }
    
    }
}