using UnityEngine;

namespace GameDevKit.UI
{
    public struct OverlayBgDim
    {
        public GameObject gameObject;
        public readonly Transform transform => gameObject.transform;
        public Canvas canvas;

        public readonly void PositionBelowUI(UIView view, int indexShift = 0)
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
    }
}