using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit.Pool
{
    public class PoolableObject : MonoBehaviour, IObjectPoolEventReceiver
    {
        [field: SerializeField] public ComponentCache ComponentCache { get; private set; }

        [field: SerializeField] public UnityEvent OnGet { get; private set; }
        [field: SerializeField] public UnityEvent OnRelease { get; private set; }

        public void HandleOnGet() => OnGet?.Invoke();
        public void HandleOnRelease() => OnRelease?.Invoke();
    }
}
