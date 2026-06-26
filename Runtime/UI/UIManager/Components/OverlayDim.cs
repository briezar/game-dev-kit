using System;
using PrimeTween;
using UnityEngine;

namespace GameDevKit.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class OverlayDim : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;

        public Canvas canvas => _canvas;
        public CanvasGroup canvasGroup => _canvasGroup;

        private void Reset()
        {
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void PositionBelowUI(UIView view, int indexShift = 0)
        {
            var active = gameObject.activeSelf;
            gameObject.SetActive(true);

            var overrideSorting = view.canvas.overrideSorting;
            canvas.overrideSorting = overrideSorting;
            canvas.sortingOrder = view.canvas.sortingOrder - 1;
            canvas.sortingLayerID = view.canvas.sortingLayerID;

            var targetSiblingIndex = view.transform.GetSiblingIndex() - 1 + indexShift;

            var parent = view.transform.parent;
            transform.SetParent(parent, false);

            if (targetSiblingIndex > 0)
            {
                transform.SetSiblingIndex(targetSiblingIndex);
            }
            else
            {
                transform.SetAsFirstSibling();
            }

            gameObject.SetActive(active);
        }

        public Tween Fade(float normalizedValue, float duration = AnimationTime.DefaultTransitionDuration)
        {
            gameObject.SetActive(true);
            var tween = Tween.Alpha(canvasGroup, normalizedValue, duration).OnComplete(() => gameObject.SetActive(normalizedValue > 0));
            return tween;
        }
    }
}