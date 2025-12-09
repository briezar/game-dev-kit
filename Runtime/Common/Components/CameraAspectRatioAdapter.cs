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

        public Camera Camera { get; private set; }

        public float BaseOrthographicSize => _referenceHeight / _pixelsPerUnit / 2f;

        private void Awake()
        {
            Camera = GetComponent<Camera>();
        }

        private void Start()
        {
            UpdateCameraSize();
        }

        [Button]
        public void UpdateCameraSize()
        {
            Camera ??= GetComponent<Camera>();

            float targetAspect = _referenceWidth / _referenceHeight;
            float windowAspect = (float)Screen.width / Screen.height;

            if (windowAspect < targetAspect)
            {
                // Screen is narrower than reference, match width
                float sizeDifference = targetAspect / windowAspect;
                Camera.orthographicSize = BaseOrthographicSize * sizeDifference;
            }
            else
            {
                // Screen is equal or wider than reference, match height
                Camera.orthographicSize = BaseOrthographicSize;
            }
        }
    }

}
