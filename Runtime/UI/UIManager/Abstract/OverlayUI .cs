using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        public virtual void Click_Close()
        {
            UIManager.HideUI(this);
            OnClose?.Invoke();
            OnClose = null;
        }

    }
}
