using UnityEngine;

namespace GameDevKit
{
    public abstract class AdvancedBehaviour : MonoBehaviour
    {
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
        protected virtual void OnStartOrEnable() { }

    }
}