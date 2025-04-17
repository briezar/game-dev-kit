using UnityEngine;
using UnityEngine.UI;

namespace GameDevKit.UI
{
    [ExecuteInEditMode]
    public class CanvasScalerPortrait : MonoBehaviour
    {
        private CanvasScaler _scaler;

#if UNITY_EDITOR
        private int _screenW;
        private int _screenH;
#endif

        private void Awake()
        {
            _scaler = GetComponent<CanvasScaler>();
            UpdateRatio();
        }

#if UNITY_EDITOR
        // Update is called once per frame
        void Update()
        {
            if (_screenW != Screen.width || _screenH != Screen.height)
            {
                UpdateRatio();
            }
        }
#endif

        void UpdateRatio()
        {
#if UNITY_EDITOR
            _screenW = Screen.width;
            _screenH = Screen.height;
#endif
            var referenceRatio = _scaler.referenceResolution.x / _scaler.referenceResolution.y;
            var ratio = (float)Screen.width / Screen.height;
            _scaler.matchWidthOrHeight = ratio >= referenceRatio ? 1f : 0f;
        }
    }
}