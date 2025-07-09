using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    [RequireComponent(typeof(Camera))]
    public class CameraSizer : MonoBehaviour
    {
        [SerializeField] private float _pixelsPerUnit = 100f;
        [SerializeField] private float _referenceWidth = 1080f;
        [SerializeField] private float _referenceHeight = 1920f;

        private void Start()
        {
            if (!TryGetComponent(out Camera camera))
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
                camera.orthographicSize = baseOrthographicSize * differenceInSize;
            }
            else
            {
                // Screen is equal or wider than reference, match height
                camera.orthographicSize = baseOrthographicSize;
            }
        }
    }

}
