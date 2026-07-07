using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit.EventProxies
{
    public class EventProxy_LifeCycle : AdvancedBehaviour
    {
        public UnityEvent onStartOrEnable;
        public UnityEvent onDisable;
        public UnityEvent onStart;
        public UnityEvent onDestroy;

        protected override void OnStart() => onStart?.Invoke();
        protected override void OnStartOrEnable() => onStartOrEnable?.Invoke();
        private void OnDisable() => onDisable?.Invoke();
        private void OnDestroy() => onDestroy?.Invoke();
    }
}