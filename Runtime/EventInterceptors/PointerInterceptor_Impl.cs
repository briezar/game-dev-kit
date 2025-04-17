using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameDevKit.EventSystems
{
    public partial class PointerInterceptor
    {
        private abstract class PointerHandler : MonoBehaviour
        {
            public Action<PointerEventData> OnPointerHandled;
        }

        private class OnPointerEnterInterceptor : PointerHandler, IPointerEnterHandler
        {
            void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class OnPointerExitInterceptor : PointerHandler, IPointerExitHandler
        {
            void IPointerExitHandler.OnPointerExit(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class OnPointerDownInterceptor : PointerHandler, IPointerDownHandler
        {
            void IPointerDownHandler.OnPointerDown(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class OnPointerUpInterceptor : PointerHandler, IPointerUpHandler
        {
            void IPointerUpHandler.OnPointerUp(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class OnPointerClickInterceptor : PointerHandler, IPointerClickHandler
        {
            void IPointerClickHandler.OnPointerClick(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class OnBeginDragInterceptor : PointerHandler, IBeginDragHandler
        {
            void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class OnDragInterceptor : PointerHandler, IDragHandler
        {
            void IDragHandler.OnDrag(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class OnEndDragInterceptor : PointerHandler, IEndDragHandler
        {
            void IEndDragHandler.OnEndDrag(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }
    }
}
