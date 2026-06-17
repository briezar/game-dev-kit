using System;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

#if UNITY_CINEMACHINE
using Unity.Cinemachine;
#endif

namespace GameDevKit
{
    /// <summary>
    /// Adjusts the camera's orthographic size to maintain a consistent aspect ratio across multiple devices based on reference dimensions.
    /// </summary>
    public class CameraAspectRatioAdapter : MonoBehaviour
    {
#if UNITY_CINEMACHINE
        [SerializeField] private CinemachineCamera _vCam;
#endif

        [SerializeField] private Camera _cam;

        public float ReferenceWidth = 1080f;
        public float ReferenceHeight = 1920f;

        /// <summary> If true, use the camera's current orthographic size as the base size. If false, use the specified BaseOrthographicSize.</summary>
        public bool UseCameraOrtho;

        [HideField(nameof(UseCameraOrtho))]
        public float BaseOrthographicSize = 9.6f;

#if UNITY_EDITOR
        private void Reset()
        {
#if UNITY_CINEMACHINE
            _vCam = GetComponentInChildren<CinemachineCamera>();
#endif
            _cam = GetComponentInChildren<Camera>();
        }
#endif

        private void Start()
        {
            if (UseCameraOrtho)
            {
                BaseOrthographicSize = GetCameraOrthoSize();
            }

            UpdateCameraSize();
        }

        [Button]
        private void CalculateSize(float pixelsPerUnit)
        {
            var orthographicSize = ReferenceHeight / pixelsPerUnit / 2f;
            Debug.Log(orthographicSize);
        }

        [Button]
        public void UpdateCameraSize()
        {
            var targetAspect = ReferenceWidth / ReferenceHeight;
            var windowAspect = (float)Screen.width / Screen.height;

            // If screen is narrower than reference, match width
            // If screen is equal or wider than reference, match height
            var sizeDifference = windowAspect < targetAspect ? targetAspect / windowAspect : 1;

            SetCameraOrthoSize(BaseOrthographicSize * sizeDifference);
        }

        private float GetCameraOrthoSize()
        {
#if UNITY_CINEMACHINE
            if (_vCam != null)
            {
                return _vCam.Lens.OrthographicSize;
            }
#endif

            if (_cam != null)
            {
                return _cam.orthographicSize;
            }

            return 0;
        }

        private void SetCameraOrthoSize(float size)
        {
#if UNITY_CINEMACHINE
            if (_vCam != null)
            {
                _vCam.Lens.OrthographicSize = size;
            }
#endif

            if (_cam != null)
            {
                _cam.orthographicSize = size;
            }
        }
    }

}
