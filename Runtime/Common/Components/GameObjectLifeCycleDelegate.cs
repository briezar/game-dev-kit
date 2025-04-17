using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit
{
    [ExecuteInEditMode]
    public class GameObjectLifeCycleDelegate : MonoBehaviour
    {
        public UnityEvent Enabled;
        public UnityEvent Disabled;
        public UnityEvent Started;
        public UnityEvent Destroyed;

        private void OnEnable()
        {
            Enabled?.Invoke();
        }

        private void OnDisable()
        {
            Disabled?.Invoke();
        }

        private void Start()
        {
            Started?.Invoke();
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke();
        }
    }
}