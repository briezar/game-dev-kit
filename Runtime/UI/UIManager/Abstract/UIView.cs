using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace GameDevKit.UI
{
    public interface IUIView
    {
        UniTask OnShow();
        UniTask OnHide();
    }

    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class UIView : AdvancedBehaviour, IUIView
    {
        [field: SerializeField] public bool DestroyOnHide { get; private set; }

        public RectTransform rectTransform => (RectTransform)transform;

        private Canvas _cacheCanvas;
        public Canvas canvas => _cacheCanvas ??= GetComponent<Canvas>();

        /// <summary> Used to wait for animation and to block user interaction when transitioning </summary>
        public virtual float TransitionInDuration => AnimationTime.DefaultTransitionDuration;

        /// <summary> Used to wait for animation and to block user interaction when transitioning </summary>
        public virtual float TransitionOutDuration => 0;

        /// <summary> Called every ViewManager.Show() </summary>
        protected virtual UniTask OnShow() => UniTask.CompletedTask;
        protected virtual UniTask OnHide() => UniTask.CompletedTask;

        UniTask IUIView.OnShow() => OnShow();
        UniTask IUIView.OnHide() => OnHide();

    }
}