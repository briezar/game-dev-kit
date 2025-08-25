using UnityEngine;

namespace GameDevKit
{
    public abstract class ImprovedBehaviour : MonoBehaviour
    {
        // Leaving this as protected instead of private to warn inheriting classes
        protected void OnEnable()
        {
            if (didStart) { OnStartOrEnable(); }
        }

        protected void Start()
        {
            OnStart();
            OnStartOrEnable();
        }

        protected virtual void OnStart() { }

        /// <summary>
        /// OnEnable will be called before Start, which can lead to race condition or null references. <br/>
        /// This method ensures OnEnable logic is always called after Start.
        /// </summary>
        protected virtual void OnStartOrEnable() { }

    }
}