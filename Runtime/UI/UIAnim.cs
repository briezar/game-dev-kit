using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GameDevKit.UI
{
    public class UIAnim : MonoBehaviour
    {
        private CanvasGroup _cacheCanvasGroup;

        public RectTransform rectTransform => transform as RectTransform;
        public CanvasGroup canvasGroup => _cacheCanvasGroup ??= this.GetOrAddComponent<CanvasGroup>();

        private Vector3 _originalScale;

        private readonly HashSet<string> _tweenIds = new();

        /// <summary> (Fade in) + (EaseOutBack scale)  </summary>
        public Sequence PlayAppear(float duration = AnimationTime.DefaultTransitionDuration, float scaleFactor = 1.5f)
        {
            var tweenId = GetTweenId(nameof(PlayAppear));
            _tweenIds.Add(tweenId);

            if (_originalScale == Vector3.zero) { _originalScale = transform.localScale; }

            canvasGroup.alpha = 0;
            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => { KillOtherTweensAndActivate(tweenId); })
            .Append(canvasGroup.DOFade(1, duration * 0.5f))
            .Join(transform.DOScale(_originalScale, duration).ChangeStartValue(_originalScale * scaleFactor).SetEase(Ease.OutBack))
            .SetId(tweenId)
            .SetLink(gameObject);

            return sequence;
        }

        /// <summary> (Fade out) + (Scale)  </summary>
        public Sequence PlayDisappear(float duration = AnimationTime.DefaultTransitionDuration, float scaleFactor = 1.25f)
        {
            var tweenId = GetTweenId(nameof(PlayDisappear));
            _tweenIds.Add(tweenId);

            if (_originalScale == Vector3.zero) { _originalScale = transform.localScale; }

            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => { KillOtherTweensAndActivate(tweenId); })
            .Append(canvasGroup.DOFade(0, duration * 0.95f))
            .Join(transform.DOScale(_originalScale * scaleFactor, duration))
            .AppendCallback(() => gameObject.SetActive(false))
            .SetId(tweenId)
            .SetLink(gameObject);

            return sequence;
        }

        public Sequence FadeIn(float duration = AnimationTime.DefaultTransitionDuration, bool useCurrentAlpha = false)
        {
            var tweenId = GetTweenId(nameof(FadeIn));
            _tweenIds.Add(tweenId);

            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => KillOtherTweensAndActivate(tweenId))
            .Append(canvasGroup.DOFade(1, duration).ChangeStartValue(useCurrentAlpha ? canvasGroup.alpha : 0))
            .SetId(tweenId)
            .SetLink(gameObject);

            return sequence;
        }

        public Sequence FadeOut(float duration = AnimationTime.DefaultTransitionDuration, bool useCurrentAlpha = true)
        {
            var tweenId = GetTweenId(nameof(FadeOut));
            _tweenIds.Add(tweenId);

            var sequence = DOTween.Sequence();
            sequence.AppendCallback(() => KillOtherTweensAndActivate(tweenId))
            .Append(canvasGroup.DOFade(0, duration).ChangeStartValue(useCurrentAlpha ? canvasGroup.alpha : 1))
            .AppendCallback(() => gameObject.SetActive(false))
            .SetId(tweenId)
            .SetLink(gameObject);

            return sequence;
        }

        private string GetTweenId(string animName)
        {
            var id = gameObject.GetInstanceID() + animName;
            return id;
        }

        private void KillOtherTweensAndActivate(string currentAnim)
        {
            foreach (var id in _tweenIds)
            {
                if (id == currentAnim) { continue; }
                DOTween.Kill(GetTweenId(id));
            }
            gameObject.SetActive(true);
        }

    }
}