using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameDevKit.EventSystems
{
    public partial class EventSystemHandler
    {
        private abstract class HandlerInstance : MonoBehaviour
        {
            public Action<PointerEventData> OnPointerHandled;
        }

        private class PointerEnterHandler : HandlerInstance, IPointerEnterHandler
        {
            void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class PointerExitHandler : HandlerInstance, IPointerExitHandler
        {
            void IPointerExitHandler.OnPointerExit(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class PointerDownHandler : HandlerInstance, IPointerDownHandler
        {
            void IPointerDownHandler.OnPointerDown(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class PointerUpHandler : HandlerInstance, IPointerUpHandler
        {
            void IPointerUpHandler.OnPointerUp(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class PointerClickHandler : HandlerInstance, IPointerClickHandler
        {
            void IPointerClickHandler.OnPointerClick(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class BeginDragHandler : HandlerInstance, IBeginDragHandler
        {
            void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class DragHandler : HandlerInstance, IDragHandler
        {
            void IDragHandler.OnDrag(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }

        private class EndDragHandler : HandlerInstance, IEndDragHandler
        {
            void IEndDragHandler.OnEndDrag(PointerEventData eventData) => OnPointerHandled?.Invoke(eventData);
        }
    }
}
