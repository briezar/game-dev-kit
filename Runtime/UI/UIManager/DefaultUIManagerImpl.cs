﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using EditorAttributes;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using GameDevKit.Editor;
#endif

namespace GameDevKit.UI
{
    public class DefaultUIManagerImpl : MonoBehaviour, IUIManager
    {
        [SerializeField] private Camera _uiCamera;
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private Canvas _screenUILayer, _overlayUILayer;
        [SerializeField] private OverlayDim _overlayDim;
        [SerializeField] private ScreenFader _screenFader;

        [HelpBox("Create via " + UIResourceMapSO.MenuName, drawAbove: true)]
        [SerializeField] private UIResourceMapSO _uiResourceMap;

        public Camera UICamera => _uiCamera;
        public EventSystem EventSystem => _eventSystem;
        public bool IsInteractable => EventSystem.isActiveAndEnabled;
        public Vector2 ReferenceResolution { get; private set; }
        public float DefaultCameraOrthoSize => ReferenceResolution.y * 0.5f * 0.01f;

        public UIView CurrentView => _views.LastOrDefault();
        public UIView PreviousView => _views.Count < 2 ? null : _views[^2];

        private readonly List<UIView> _views = new();

        private readonly Dictionary<Type, List<UIView>> _instantiatedViewMap = new();

        private int _blockCount = 0;

        private void Awake()
        {
            // https://developer.android.com/training/multiscreen/screendensities#dips-pels
            var defaultDragThreshold = EventSystem.pixelDragThreshold;
            var acceptableDragThreshold = (int)(defaultDragThreshold * (Screen.dpi / 160f));
            EventSystem.pixelDragThreshold = Mathf.Max(defaultDragThreshold, acceptableDragThreshold);

            ReferenceResolution = GetComponentInChildren<CanvasScaler>().referenceResolution;

            var layers = new Transform[] { _screenUILayer.transform, _overlayUILayer.transform };

            foreach (var view in layers.SelectMany(layer => layer.GetComponentsInChildren<UIView>(true)))
            {
                AddInstantiatedUI(view);

                if (!view.didAwake)
                {
                    view.gameObject.SetActive(true);
                    view.gameObject.SetActive(false);
                }
            }

            UIManager.SetImplementation(this);
        }

        private Transform GetLayer(UIView view) => view switch
        {
            ScreenUI => _screenUILayer.transform,
            OverlayUI => _overlayUILayer.transform,
            _ => _screenUILayer.transform,
        };

#if UNITY_EDITOR
        [Button]
        private void EnsureSortingLayer()
        {
            var canvasLayers = new Canvas[] { _screenUILayer, _overlayUILayer, _overlayDim.canvas, _screenFader.canvas };

            foreach (var canvasLayer in canvasLayers)
            {
                EditorUtils.CreateSortingLayer(canvasLayer.sortingLayerName, canvasLayer.sortingLayerID);
            }
        }

        private List<string> _setInteractableCallers = new();
        [Button]
        private void CheckInteractableCaller()
        {
            Debug.Log(_setInteractableCallers.JoinToString("\n"));
            _setInteractableCallers.Clear();
        }
#endif

        public async UniTask DisableInteractionFor(float durationSeconds)
        {
            SetInteractable(false);
            await UniTask.WaitForSeconds(durationSeconds);
            SetInteractable(true);
        }

        public void SetInteractable(bool interactable, bool force = false)
        {
#if UNITY_EDITOR
            var collectStackTrace = true;
            if (collectStackTrace)
            {
                try
                {
                    var callerInfo = GeneralUtils.GetMethodCallerInfo();
                    var interactableState = interactable.ToString().Colorize(interactable ? Color.green : Color.red);

                    callerInfo += $", SetInteractable({interactableState}) - [{DateTime.Now:HH:mm:ss.F2}]";

                    _setInteractableCallers.AddFirst(callerInfo);
                    if (_setInteractableCallers.Count > 10)
                    {
                        _setInteractableCallers.RemoveLast();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }
#endif

            if (force)
            {
                _blockCount = 0;
                EventSystem.enabled = interactable;
                return;
            }

            if (!interactable)
            {
                EventSystem.enabled = false;
                _blockCount++;
                return;
            }

            _blockCount--;

            if (_blockCount <= 0)
            {
                _blockCount = 0;
                EventSystem.enabled = true;
            }
        }

        public UniTask FadeTransition(FadeSetting fadeSetting) => _screenFader.FadeTransition(fadeSetting);

        private void AddInstantiatedUI(UIView view)
        {
            var viewType = view.GetType();
            if (_instantiatedViewMap.TryGetValue(viewType, out var views))
            {
                views.Add(view);
                return;
            }

            _instantiatedViewMap[viewType] = new() { view };
        }

        private void RemoveInstantiatedUI(UIView view)
        {
            var viewType = view.GetType();
            if (_instantiatedViewMap.TryGetValue(viewType, out var views))
            {
                views.Remove(view);
            }
        }

        private bool TryGetInstantiatedUI<T>(out T view) where T : UIView, new()
        {
            view = null;
            var viewType = typeof(T);
            if (_instantiatedViewMap.TryGetValue(viewType, out var views) && views.Count > 0)
            {
                view = views[^1] as T;
                return true;
            }

            return false;
        }

        public T PreloadUI<T>() where T : UIView, new()
        {
            var view = GetOrInstantiateUI<T>();
            if (view == null) { return null; }
            return view;
        }

        public async UniTask ShowUI(UIView view)
        {
            var viewName = view.GetType().Name;
            if (view == CurrentView)
            {
                Debug.LogWarning($"Showing the current view again: [{viewName}]");
                return;
            }

            _views.Add(view);
            view.transform.SetAsLastSibling();

            if (view is OverlayUI overlayUI)
            {
                if (overlayUI.ShowBehaviour.HasFlagFast(ShowOverlayBehaviour.DimLowerUI))
                {
                    _overlayDim.PositionBelowUI(overlayUI);
                    _overlayDim.Fade(1);
                }
            }

            view.gameObject.SetActive(true);
            var showTask = (view as IUIView).OnShow();

            SetInteractable(false);
            await showTask;
            SetInteractable(true);
        }

        public T ShowUI<T>() where T : UIView, new()
        {
            var view = GetOrInstantiateUI<T>();
            if (view == null) { return null; }

            ShowUI(view);
            return view;
        }

        public async UniTask HideUI<T>(bool immediate = false) where T : UIView, new()
        {
            var viewName = typeof(T).Name;
            if (!TryGetInstantiatedUI<T>(out var baseView))
            {
                Debug.LogWarning($"View does not exist: {viewName}");
                return;
            }
            await HideUI(baseView, immediate);
        }

        public async UniTask HideUI(UIView view, bool immediate = false)
        {
            if (view == null)
            {
                Debug.LogWarning($"[ViewManager] Trying to hide a null UI");
                return;
            }

            if (_views.Count < 2)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[ViewManager] Trying to hide all UIs, ignore this message if hiding {view.GetType().Name} was intended");
#endif
            }

            _views.RemoveAll(v => v == null || v.gameObject == null || v == view);

            var hideTask = (view as IUIView).OnHide();

            for (int i = _views.Count - 1; i >= 0; i--)
            {
                if (_views[i] is OverlayUI upperOverlayUI && upperOverlayUI.ShowBehaviour.HasFlagFast(ShowOverlayBehaviour.DimLowerUI))
                {
                    _overlayDim.PositionBelowUI(upperOverlayUI);
                    break;
                }

                if (i == 0)
                {
                    if (view is OverlayUI firstOverlayUI && firstOverlayUI.ShowBehaviour.HasFlagFast(ShowOverlayBehaviour.DimLowerUI))
                    {
                        _overlayDim.Fade(0);
                    }
                }
            }

            if (!immediate)
            {
                SetInteractable(false);
                await hideTask;
                SetInteractable(true);
            }

            if (view.DestroyOnHide)
            {
                if (view != null && view.gameObject != null) { Destroy(view.gameObject); }
                RemoveInstantiatedUI(view);
            }
            else
            {
                view.gameObject.SetActive(false);
            }
        }

        private T GetOrInstantiateUI<T>() where T : UIView, new()
        {
            if (TryGetInstantiatedUI<T>(out var view))
            {
                if (view is not OverlayUI overlayUI || !overlayUI.CanShowMultiple)
                {
                    return view;
                }
            }

            view = InstantiateUI<T>();
            return view;
        }


        private T InstantiateUI<T>() where T : UIView, new()
        {
            var resource = _uiResourceMap.LoadResource<T>();
            if (resource == null) { return null; }

            var view = Instantiate(resource, GetLayer(resource));
            AddInstantiatedUI(view);
            return view;
        }

    }
}