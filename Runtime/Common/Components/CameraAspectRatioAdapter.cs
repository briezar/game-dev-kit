using System;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    /// <summary>
    /// Adjusts the camera's orthographic size to maintain a consistent aspect ratio across multiple devices based on reference dimensions.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraAspectRatioAdapter : MonoBehaviour
    {
        [SerializeField] private float _pixelsPerUnit = 100f;
        [SerializeField] private float _referenceWidth = 1080f;
        [SerializeField] private float _referenceHeight = 1920f;

        private Camera _cam;

        public float BaseOrthographicSize => _referenceHeight / _pixelsPerUnit / 2f;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void Start()
        {
            UpdateCameraSize();
        }

        [Button]
        public void UpdateCameraSize()
        {
            if (_cam == null && !TryGetComponent(out _cam))
            {
                Debug.LogError("CameraSizer requires a Camera component.");
                return;
            }

            float targetAspect = _referenceWidth / _referenceHeight;
            float windowAspect = (float)Screen.width / Screen.height;

            if (windowAspect < targetAspect)
            {
                // Screen is narrower than reference, match width
                float differenceInSize = targetAspect / windowAspect;
                _cam.orthographicSize = BaseOrthographicSize * differenceInSize;
            }
            else
            {
                // Screen is equal or wider than reference, match height
                _cam.orthographicSize = BaseOrthographicSize;
            }
        }
    }

}
