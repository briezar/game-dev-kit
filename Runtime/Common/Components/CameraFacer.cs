using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    public class CameraFacer : AdvancedBehaviour
    {
        [HelpBox("Defaults to Camera with Tag if not assigned")]
        public Camera TargetCamera;

        [TagDropdown]
        public string Tag = "MainCamera";

        public bool FaceOnEnable = true;
        public bool UseUpdate = true;

        protected override void OnStartOrEnable()
        {
            if (TargetCamera == null)
            {
                TargetCamera = Camera.allCameras.Find(c => c.CompareTag(Tag));
            }
            if (FaceOnEnable) { FaceCamera(); }
        }

        private void LateUpdate()
        {
            // LateUpdate is preferred for camera-related movements to ensure the camera has finished its own movement for the frame.
            if (UseUpdate)
            {
                FaceCamera();
            }
        }

        [Button]
        public void FaceCamera()
        {
            if (TargetCamera == null) { return; }

            transform.rotation = Quaternion.LookRotation(TargetCamera.transform.forward);
        }
    }
}