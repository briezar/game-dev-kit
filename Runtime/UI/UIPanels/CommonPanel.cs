using System;
using System.Collections;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

namespace GameDevKit.UI
{
    public enum TransitionDirection { None, LeftToRight, RightToLeft }

    public abstract class CommonPanel<T> : ImprovedBehaviour where T : CommonPanel<T>
    {
        private UIAnim _cacheUiAnim;
        public UIAnim Anim => _cacheUiAnim ??= this.GetOrAddComponent<UIAnim>();
        public CanvasGroup canvasGroup => Anim.canvasGroup;
        public RectTransform rectTransform => transform as RectTransform;

        private int _index = -1;

        public Action OnClickNext, OnClickBack;
        public T PrevPanel { get; protected set; }
        public T NextPanel { get; protected set; }
        public bool CanBack { get; set; } = true;

        public int PanelIndex => _index < 0 ? transform.GetSiblingIndex() : _index;

        /// <summary> -1 will default to SiblingIndex </summary>
        public void SetIndex(int index)
        {
            _index = index;
        }

        public virtual void OnShow()
        {
            gameObject.SetActive(true);
        }
        public virtual void OnHide()
        {
            gameObject.SetActive(false);
        }


        public void SetNavigation(T prevPanel, T nextPanel)
        {
            PrevPanel = prevPanel;
            NextPanel = nextPanel;
        }

        public virtual void TransitionIn(TransitionDirection direction = TransitionDirection.None, float duration = AnimationTime.DefaultTransitionDuration)
        {
            // gameObject.SetActive(true);

            // if (direction != TransitionDirection.None)
            // {
            //     var baseOffset = rectTransform.rect.width;
            //     var xOffset = baseOffset * (direction == TransitionDirection.LeftToRight ? -1 : 1);
            //     rectTransform.anchoredPosition = new Vector3(xOffset, 0);

            //     rectTransform.DOAnchorPosX(0, duration);
            // }
            // Anim.FadeIn(duration);
        }

        public virtual void TransitionOut(TransitionDirection direction = TransitionDirection.None, float duration = AnimationTime.DefaultTransitionDuration)
        {
            // gameObject.SetActive(true);
            // if (direction != TransitionDirection.None)
            // {
            //     var baseOffset = rectTransform.rect.width;
            //     var xOffset = baseOffset * (direction == TransitionDirection.LeftToRight ? 1 : -1);
            //     rectTransform.anchoredPosition = Vector3.zero;

            //     rectTransform.DOAnchorPosX(xOffset, duration);
            // }
            // Anim.FadeOut(duration);
        }

#if UNITY_EDITOR
        [Button]
        private void Select()
        {
            foreach (var panel in transform.parent.GetComponentsInChildren<CommonPanel<T>>(true))
            {
                panel.gameObject.SetActive(panel == this);
            }
        }
#endif
    }
}