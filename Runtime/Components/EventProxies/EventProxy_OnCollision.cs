using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit.EventProxies
{
    [Serializable]
    public class CollisionEvent : UnityEvent<Collision> { }

    [RequireComponent(typeof(Collider))]
    public class EventProxy_OnCollision : MonoBehaviour
    {
        public CollisionEvent onEnter;
        public CollisionEvent onStay;
        public CollisionEvent onExit;

        public bool FireWhenDisabled = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onEnter?.Invoke(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onStay?.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onExit?.Invoke(collision);
        }
    }
}