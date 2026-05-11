using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    public class CameraFacer : AdvancedBehaviour
    {
        public enum FacingMode
        {
            Billboard,      // Faces camera directly (world UI, sprites)
            UprightToward,  // Y-only rotation toward camera position (perspective signs)
            UprightForward, // Y-only rotation matching camera forward (isometric signs)
        }

        [HelpBox("Defaults to Camera with Tag if not assigned")]
        public Camera TargetCamera;

        [TagDropdown]
        public string Tag = "MainCamera";

        public FacingMode Mode = FacingMode.Billboard;
        public Vector3 Offset;

        [Tooltip("Applied after facing, use to correct objects that face backwards")]
        public float RotationOffset = 0f;

        public bool FaceOnEnable = true;
        public bool UseUpdate = true;

        protected override void OnStartOrEnable()
        {
            if (TargetCamera == null) { TargetCamera = Camera.allCameras.Find(c => c.CompareTag(Tag)); }
            if (FaceOnEnable) { FaceCamera(); }
        }

        private void LateUpdate()
        {
            if (UseUpdate) { FaceCamera(); }
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

                case FacingMode.UprightToward:
                    // Points toward camera position — varies per world position (perspective)
                    var toCamera = TargetCamera.transform.position - transform.position;
                    toCamera.y = 0f;
                    if (toCamera.sqrMagnitude > 0.001f) { transform.rotation = Quaternion.LookRotation(-toCamera.normalized); }
                    break;

                case FacingMode.UprightForward:
                    // Mirrors camera forward on XZ — identical for all objects (isometric)
                    var forward = TargetCamera.transform.forward;
                    forward.y = 0f;
                    if (forward.sqrMagnitude > 0.001f) { transform.rotation = Quaternion.LookRotation(forward.normalized); }
                    break;
            }

            var euler = transform.eulerAngles + Offset;
            euler.y += RotationOffset;
            transform.eulerAngles = euler;
        }
    }
}