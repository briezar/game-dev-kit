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
        public TriggerEvent onEnter;
        public TriggerEvent onStay;
        public TriggerEvent onExit;

        public bool FireWhenDisabled = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onEnter?.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onStay?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onExit?.Invoke(other);
        }
    }
}