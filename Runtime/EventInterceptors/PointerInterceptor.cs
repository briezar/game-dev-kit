using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameDevKit.EventSystems
{
    public partial class PointerInterceptor : MonoBehaviour
    {
        private OnPointerEnterInterceptor _pointerEnter;
        private OnPointerExitInterceptor _pointerExit;
        private OnPointerDownInterceptor _pointerDown;
        private OnPointerUpInterceptor _pointerUp;
        private OnPointerClickInterceptor _click;
        private OnBeginDragInterceptor _beginDrag;
        private OnDragInterceptor _drag;
        private OnEndDragInterceptor _endDrag;

        private readonly HashSet<PointerHandler> _pointerHandlers = new();

        private void OnDestroy()
        {
            foreach (var handler in _pointerHandlers)
            {
                Destroy(handler);
            }
        }

        public Action<PointerEventData> OnPointerEnter
        {
            get => LazyGet(ref _pointerEnter).OnPointerHandled;
            set => LazyGet(ref _pointerEnter).OnPointerHandled = value;
        }
        public Action<PointerEventData> OnPointerExit
        {
            get => LazyGet(ref _pointerExit).OnPointerHandled;
            set => LazyGet(ref _pointerExit).OnPointerHandled = value;
        }
        public Action<PointerEventData> OnPointerDown
        {
            get => LazyGet(ref _pointerDown).OnPointerHandled;
            set => LazyGet(ref _pointerDown).OnPointerHandled = value;
        }
        public Action<PointerEventData> OnPointerUp
        {
            get => LazyGet(ref _pointerUp).OnPointerHandled;
            set => LazyGet(ref _pointerUp).OnPointerHandled = value;
        }
        public Action<PointerEventData> OnClick
        {
            get => LazyGet(ref _click).OnPointerHandled;
            set => LazyGet(ref _click).OnPointerHandled = value;
        }
        public Action<PointerEventData> OnBeginDrag
        {
            get => LazyGet(ref _beginDrag).OnPointerHandled;
            set => LazyGet(ref _beginDrag).OnPointerHandled = value;
        }
        public Action<PointerEventData> OnDrag
        {
            get => LazyGet(ref _drag).OnPointerHandled;
            set => LazyGet(ref _drag).OnPointerHandled = value;
        }
        public Action<PointerEventData> OnEndDrag
        {
            get => LazyGet(ref _endDrag).OnPointerHandled;
            set => LazyGet(ref _endDrag).OnPointerHandled = value;
        }

        private T LazyGet<T>(ref T backingField) where T : PointerHandler
        {
            if (backingField == null || backingField.Equals(null))
            {
                backingField = gameObject.GetOrAddComponent<T>();
            }
            _pointerHandlers.Add(backingField);
            return backingField;
        }
    }



}
