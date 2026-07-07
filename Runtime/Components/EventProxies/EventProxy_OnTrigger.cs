using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit.EventProxies
{
    [Serializable]
    public class TriggerEvent : UnityEvent<Collider> { }

    [RequireComponent(typeof(Collider))]
    public class EventProxy_OnTrigger : MonoBehaviour
    {
        public TriggerEvent OnEnter;
        public TriggerEvent OnStay;
        public TriggerEvent OnExit;

        private void OnTriggerEnter(Collider other) => OnEnter?.Invoke(other);
        private void OnTriggerStay(Collider other) => OnStay?.Invoke(other);
        private void OnTriggerExit(Collider other) => OnExit?.Invoke(other);
    }
}