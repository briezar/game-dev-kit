using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using GameDevKit.Attributes;

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

        [SerializeReference, SubclassPicker] private UIAnimationInstruction.Instruction _showAnim, _hideAnim;

        public RectTransform rectTransform => (RectTransform)transform;

        private Canvas _cacheCanvas;
        public Canvas canvas => _cacheCanvas ??= GetComponent<Canvas>();

        private UIAnimation _cacheAnim;
        public UIAnimation UIAnim => _cacheAnim ??= this.GetOrAddComponent<UIAnimation>();

        /// <summary> Called every ViewManager.Show() </summary>
        protected async virtual UniTask OnShow()
        {
            if (_showAnim != null)
            {
                await UIAnim.PlayAsync(_showAnim);
            }
        }

        protected async virtual UniTask OnHide()
        {
            if (_hideAnim != null)
            {
                await UIAnim.PlayAsync(_hideAnim);
            }
        }

        UniTask IUIView.OnShow() => OnShow();

        UniTask IUIView.OnHide() => OnHide();

    }
}