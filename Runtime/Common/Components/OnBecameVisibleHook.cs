using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit
{
    [RequireComponent(typeof(Renderer))]
    public class OnBecameVisibleHook : MonoBehaviour
    {
        [field: SerializeField] public UnityEvent OnVisible { get; private set; } = new();
        [field: SerializeField] public UnityEvent OnInvisible { get; private set; } = new();

        public CameraType ValidCameras = CameraType.Game;

        public bool IsVisible { get; private set; } = false;

        private int _visibleCameraCount = 0;

        private bool TryGetValidCamera(out Camera cam)
        {
            cam = Camera.current;
            return cam != null && (ValidCameras & cam.cameraType) != 0;
        }

        private void OnBecameVisible()
        {
            if (!TryGetValidCamera(out _)) { return; }

            _visibleCameraCount++;
            if (_visibleCameraCount == 1)
            {
                IsVisible = true;
                OnVisible?.Invoke();
            }
        }

        private void OnBecameInvisible()
        {
            if (!TryGetValidCamera(out _)) { return; }

            _visibleCameraCount = Mathf.Max(0, _visibleCameraCount - 1);
            if (_visibleCameraCount == 0)
            {
                IsVisible = false;
                OnInvisible?.Invoke();
            }
        }

        private void OnDisable()
        {
            if (_visibleCameraCount > 0)
            {
                _visibleCameraCount = 0;
                IsVisible = false;
                OnInvisible?.Invoke();
            }
        }
    }
}