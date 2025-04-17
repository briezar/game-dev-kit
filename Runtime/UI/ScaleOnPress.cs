using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace GameDevKit.UI
{
    [RequireComponent(typeof(Selectable))]
    public class ScaleOnPress : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float _scaleDownRatio = SCALE_DOWN;
        [SerializeField] private float _duration = DURATION;
        [SerializeField] private bool _scaleUpOnPointerExit = true;
        [SerializeField] private Vector3 _originalScale = Vector3.one;
        [SerializeField] private bool _useUnscaledTime = true;

        public Action OnPress, OnRelease;

        private const float DURATION = 0.04f;
        private const float SCALE_DOWN = 0.94f;

        private Vector4 _originalPadding;

        private bool _pressed = false;
        private Selectable _selectable;

#if UNITY_EDITOR
        private void Reset()
        {
            _originalScale = transform.localScale;
        }
#endif

        private void Awake()
        {
            if (!TryGetComponent(out _selectable)) { return; }

            _originalPadding = _selectable.targetGraphic != null ? _selectable.targetGraphic.raycastPadding : Vector4.zero;
        }

        private void EnablePadding(bool enable)
        {
            if (_selectable == null) { return; }

            var graphic = _selectable.targetGraphic;
            if (graphic == null)
            {
                // Debug.LogWarning(gameObject.name + " does not have graphic!");
                return;
            }
            if (!enable)
            {
                graphic.raycastPadding = _originalPadding;
                return;
            }

            var widthPadding = graphic.rectTransform.rect.width * (1 - _scaleDownRatio);
            var heightPadding = graphic.rectTransform.rect.height * (1 - _scaleDownRatio);

            var padding = graphic.raycastPadding;
            padding.x -= widthPadding;
            padding.z -= widthPadding;
            padding.y -= heightPadding;
            padding.w -= heightPadding;

            graphic.raycastPadding = padding;
        }

        public void UpdateOriginalScale()
        {
            _originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            transform.localScale = _originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_selectable && !_selectable.interactable) { return; }

            _pressed = true;
            EnablePadding(true);
            Scale(false);

            OnPress?.Invoke();
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerExit(eventData);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_selectable && !_selectable.interactable) { return; }

            _pressed = false;
            Scale(true);
            OnRelease?.Invoke();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_pressed && _scaleUpOnPointerExit) { Scale(true); }

            _pressed = false;
            EnablePadding(false);
            OnRelease?.Invoke();
        }

        private void Scale(bool up)
        {
            var scaleTo = _originalScale * (up ? 1 : _scaleDownRatio);

            transform.DOKill();
            transform.DOScale(scaleTo, _duration).SetUpdate(_useUnscaledTime).SetLink(gameObject);
        }
    }
}