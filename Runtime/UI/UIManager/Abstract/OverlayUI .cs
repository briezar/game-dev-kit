using System;
using UnityEngine;

namespace GameDevKit.UI
{
    public interface IOverlayUI
    {
        ShowOverlayBehaviour ShowBehaviour { get; }
    }

    public abstract class OverlayUI : UIView, IOverlayUI
    {
        [SerializeField] private ShowOverlayBehaviour _showBehaviour;
        [field: SerializeField] public bool CanShowMultiple { get; private set; }

        public ShowOverlayBehaviour ShowBehaviour => _showBehaviour;

        public Action OnClose;

        public virtual void Close()
        {
            UIManager.HideUI(this);
            OnClose?.Invoke();
            OnClose = null;
        }

    }
}
