using UnityEngine;

namespace GameDevKit
{
    public abstract class StaticBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;

        protected virtual void Awake()
        {
            _instance = this as T;
        }
    }

    public abstract class SingletonBehaviour<T> : StaticBehaviour<T> where T : MonoBehaviour
    {
        protected sealed override void Awake()
        {
            if (_instance != null)
            {
                OnInvalidAwake();
                return;
            }
            base.Awake();
            OnValidAwake();
        }

        protected void OnDestroy()
        {
            if (_instance != this)
            {
                OnInvalidDestroy();
                return;
            }
            _instance = null;
            OnValidDestroy();
        }

        protected virtual void OnValidAwake() { }

        protected virtual void OnInvalidAwake()
        {
            Debug.LogWarning($"Already have an instance of {GetType().Name}.");
            Destroy(gameObject);
        }

        protected virtual void OnValidDestroy() { }
        protected virtual void OnInvalidDestroy() { }
    }
}