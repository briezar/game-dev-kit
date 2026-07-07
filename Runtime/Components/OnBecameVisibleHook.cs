using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit
{
    [RequireComponent(typeof(Renderer))]
    public class OnBecameVisibleHook : MonoBehaviour
    {
        [field: SerializeField] public UnityEvent OnVisible { get; private set; } = new();
        [field: SerializeField] public UnityEvent OnInvisible { get; private set; } = new();

        private Renderer _renderer;

        public bool IsVisible
        {
            get
            {
                _renderer = _renderer ? _renderer : GetComponent<Renderer>();
                return _renderer.isVisible;
            }
        }

        private void OnBecameVisible() => OnVisible?.Invoke();

        private void OnBecameInvisible() => OnInvisible?.Invoke();

    }
}