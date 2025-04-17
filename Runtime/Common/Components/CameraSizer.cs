using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    [RequireComponent(typeof(Camera))]
    public class CameraSizer : MonoBehaviour
    {
        [SerializeField] private float _pixelsPerUnit = 100;
        
        private void Start()
        {
            if (!TryGetComponent<Camera>(out var camera))
            {
                Debug.LogError("CameraSizer requires a Camera component.");
                return;
            }

            var size = Screen.height / _pixelsPerUnit / 2f;
            camera.orthographicSize = size;
        }
    }
}
