using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit.Pool
{
    public class PoolableObject : MonoBehaviour, IPoolableObjectEventReceiver
    {
        [field: SerializeField] public ComponentCache ComponentCache { get; private set; }

        public UnityEvent OnGetEvent, OnStoreEvent;

        public void OnGet() => OnGetEvent?.Invoke();
        public void OnStore() => OnStoreEvent?.Invoke();
    }
}