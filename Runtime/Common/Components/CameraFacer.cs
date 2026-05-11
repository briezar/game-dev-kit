using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    public class CameraFacer : AdvancedBehaviour
    {
        public enum FacingMode
        {
            [Tooltip("Faces camera directly (world UI, sprites)")]
            Billboard,

            [Tooltip("Rotates toward camera on Y only (signs, props)")]
            Upright,
        }

        [HelpBox("Defaults to Camera with Tag if not assigned")]
        public Camera TargetCamera;

        [TagDropdown]
        public string Tag = "MainCamera";

        public FacingMode Mode = FacingMode.Billboard;
        public Vector3 Offset;

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
            if (UseUpdate)
            {
                FaceCamera();
            }
        }

        [Button]
        public void FaceCamera()
        {
            if (TargetCamera == null) { return; }

            switch (Mode)
            {
                case FacingMode.Billboard:
                    transform.rotation = Quaternion.LookRotation(TargetCamera.transform.forward);
                    break;

                case FacingMode.Upright:
                    // Project the direction to the camera onto the XZ plane so the
                    // object only pivots on Y and never tilts (works for both
                    // perspective and isometric cameras).
                    Vector3 toCamera = TargetCamera.transform.position - transform.position;
                    toCamera.y = 0f;

                    if (toCamera.sqrMagnitude > 0.001f)
                    {
                        transform.rotation = Quaternion.LookRotation(-toCamera.normalized);
                    }
                    break;
            }

            transform.eulerAngles += Offset;
        }
    }
}