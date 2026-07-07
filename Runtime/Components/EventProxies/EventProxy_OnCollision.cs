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

        private void OnCollisionEnter(Collision collision) => onEnter?.Invoke(collision);
        private void OnCollisionStay(Collision collision) => onStay?.Invoke(collision);
        private void OnCollisionExit(Collision collision) => onExit?.Invoke(collision);
    }
}