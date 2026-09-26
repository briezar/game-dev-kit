using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit.EventProxies
{
    /// <summary>
    /// This component allows you to raise UnityEvents from an Animator component using string key.
    /// Attach this component to a GameObject with an Animator, then in the Animation tab, add an Animation Event and set the function to <see cref="RaiseEvent"/> and pass in the desired key.
    /// This will invoke the UnityEvent associated with that key in this component.
    /// </summary>
    public class EventProxy_AnimatorEvent : EventProxy_AnimatorEvent<string>
    {

    }

    /// <summary>
    /// This component allows you to raise UnityEvents from an Animator component using a ScriptableObject of type <typeparamref name="T"/> key.
    /// Attach this component to a GameObject with an Animator, then in the Animation tab, add an Animation Event and set the function to <see cref="RaiseEvent"/> and pass in the desired key.
    /// This will invoke the UnityEvent associated with that key in this component.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public abstract class EventProxy_AnimatorEvent<T> : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<T, UnityEvent> _events;

        public UnityEvent<T> onEventRaised;

        public void RaiseEvent(T key)
        {
            if (_events.TryGetValue(key, out var unityEvent))
            {
                unityEvent?.Invoke();
            }
            onEventRaised?.Invoke(key);
        }
    }
}