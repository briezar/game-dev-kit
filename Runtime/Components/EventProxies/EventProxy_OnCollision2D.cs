using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit.EventProxies
{
    [Serializable]
    public class Collision2DEvent : UnityEvent<Collision2D> { }

    [RequireComponent(typeof(Collider2D))]
    public class EventProxy_OnCollision2D : MonoBehaviour
    {
        public Collision2DEvent onEnter;
        public Collision2DEvent onStay;
        public Collision2DEvent onExit;

        public bool FireWhenDisabled = false;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onEnter?.Invoke(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onStay?.Invoke(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!FireWhenDisabled && !enabled) { return; }
            onExit?.Invoke(collision);
        }
    }
}