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
        public Trigger2DEvent OnEnter;
        public Trigger2DEvent OnStay;
        public Trigger2DEvent OnExit;

        private void OnTriggerEnter2D(Collider2D other) => OnEnter?.Invoke(other);
        private void OnTriggerStay2D(Collider2D other) => OnStay?.Invoke(other);
        private void OnTriggerExit2D(Collider2D other) => OnExit?.Invoke(other);
    }
}