using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit.EventProxies
{
    [Serializable]
    public class Trigger2DEvent : UnityEvent<Collider2D> { }

    [RequireComponent(typeof(Collider))]
    public class EventProxy_OnTrigger2D : MonoBehaviour
    {
        public Trigger2DEvent onEnter;
        public Trigger2DEvent onStay;
        public Trigger2DEvent onExit;

        public bool FireWhenDisabled = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onEnter?.Invoke(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onStay?.Invoke(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onExit?.Invoke(other);
        }
    }
}