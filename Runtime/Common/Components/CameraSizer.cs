using System;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    [RequireComponent(typeof(Camera))]
    public class CameraSizer : MonoBehaviour
    {
        [SerializeField] private float _pixelsPerUnit = 100f;
        [SerializeField] private float _referenceWidth = 1080f;
        [SerializeField] private float _referenceHeight = 1920f;

        private Camera _cam;

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

            // Base orthographic size from the reference height
            float baseOrthographicSize = _referenceHeight / _pixelsPerUnit / 2f;

            if (windowAspect < targetAspect)
            {
                // Screen is narrower than reference, match width
                float differenceInSize = targetAspect / windowAspect;
                _cam.orthographicSize = baseOrthographicSize * differenceInSize;
            }
            else
            {
                // Screen is equal or wider than reference, match height
                _cam.orthographicSize = baseOrthographicSize;
            }
        }
    }

}
