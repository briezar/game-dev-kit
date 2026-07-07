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
        public Collision2DEvent OnEnter;
        public Collision2DEvent OnStay;
        public Collision2DEvent OnExit;

        private void OnCollisionEnter2D(Collision2D collision) => OnEnter?.Invoke(collision);
        private void OnCollisionStay2D(Collision2D collision) => OnStay?.Invoke(collision);
        private void OnCollisionExit2D(Collision2D collision) => OnExit?.Invoke(collision);
    }
}