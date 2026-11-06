using System;
using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public class CameraTagRef
    {
        [TagDropdown]
        [SerializeField] private string _tag = "MainCamera";

        public string Tag
        {
            get => _tag;
            set
            {
                _tag = value;
                _camera = null;
            }
        }

        public Camera Camera => _camera != null ? _camera : (_camera = Camera.allCameras.Find(c => c.CompareTag(Tag)));

        private Camera _camera;

        public static implicit operator Camera(CameraTagRef cameraTagRef) => cameraTagRef.Camera;
    }
}