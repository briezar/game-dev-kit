﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace GameDevKit.UI
{
    public class UIManager
    {
        private static IUIManager _impl;

        public static bool IsReady => _impl != null;
        public static Camera Camera => _impl.UICamera;
        public static EventSystem EventSystem => _impl.EventSystem;
        public static Vector2 ReferenceResolution => _impl.ReferenceResolution;
        public static float DefaultCameraOrthoSize => _impl.DefaultCameraOrthoSize;
        public static bool IsInteractable => _impl.IsInteractable;

        public static UIView CurrentView => _impl.CurrentView;
        public static UIView PreviousView => _impl.PreviousView;

        public static void SetImplementation(IUIManager impl)
        {
            Debug.Log($"SetImplementation for UIManager: [{impl.GetType().Name}]", impl as Object);
            _impl = impl;
        }

        public static void DisableInteractionFor(float durationSeconds) => _impl.DisableInteractionFor(durationSeconds);
        public static void SetInteractable(bool interactable, bool force = false) => _impl.SetInteractable(interactable, force);
        public static UniTask FadeTransition(FadeSetting fadeSetting) => _impl.FadeTransition(fadeSetting);
        public static T GetUI<T>() where T : UIView, new() => _impl.PreloadUI<T>();
        public static T PreloadUI<T>() where T : UIView, new() => _impl.PreloadUI<T>();
        public static bool IsShowing<T>() => _impl.CurrentView is T;
        public static UniTask ShowUI(UIView view) => _impl.ShowUI(view);
        public static T ShowUI<T>() where T : UIView, new() => _impl.ShowUI<T>();
        public static UniTask HideUI<T>(bool immediate = false) where T : UIView, new() => _impl.HideUI<T>(immediate);
        public static UniTask HideUI(UIView view, bool immediate = false) => _impl.HideUI(view, immediate);

    }
}