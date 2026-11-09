using System;
using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public class CameraTagRef
    {
        [TagDropdown]
        [SerializeField] private string _tag = CameraUtils.MainCameraTag;

        public string Tag
        {
            get => _tag;
            set
            {
                _tag = value;
                _camera = null;
            }
        }

        public Camera Camera => _camera != null ? _camera : (_camera = CameraUtils.FindCameraByTag(Tag));

        private Camera _camera;

        public static implicit operator Camera(CameraTagRef cameraTagRef) => cameraTagRef.Camera;
    }
}