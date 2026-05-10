using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasCameraFinder : MonoBehaviour
    {
        private Canvas _canvas;

        [TagDropdown]
        public string Tag = "MainCamera";

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            _canvas.worldCamera = Camera.allCameras.Find(c => c.CompareTag(Tag));
        }
    }
}