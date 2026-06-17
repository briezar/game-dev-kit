using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

namespace GameDevKit.UI
{
    public interface IUIManager
    {
        Camera UICamera { get; }
        EventSystem EventSystem { get; }
        Vector2 ReferenceResolution { get; }
        float DefaultCameraOrthoSize { get; }
        bool IsInteractable { get; }
        UIView CurrentView { get; }
        UIView PreviousView { get; }

        UniTask DisableInteractionFor(float durationSeconds);
        void SetInteractable(bool interactable, bool force = false);
        UniTask FadeTransition(FadeSetting fadeSetting);
        T PreloadUI<T>() where T : UIView, new();
        UniTask ShowUI(UIView view);
        T ShowUI<T>() where T : UIView, new();
        UniTask HideUI<T>(bool immediate = false) where T : UIView, new();
        UniTask HideUI(UIView view, bool immediate = false);
    }
}