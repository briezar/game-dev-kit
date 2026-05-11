using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;

namespace GameDevKit.UI
{
    public class UIAnim : MonoBehaviour
    {
        private CanvasGroup _cacheCanvasGroup;

        public CanvasGroup canvasGroup => _cacheCanvasGroup ??= this.GetOrAddComponent<CanvasGroup>();

        private Vector3? _originalScale;

        private readonly HashSet<Sequence> _runningSequences = new();

        /// <summary> (Fade in) + (EaseOutBack scale)  </summary>
        public async UniTask PlayAppear(float duration = AnimationTime.DefaultTransitionDuration, float scaleFactor = 1.5f)
        {
            if (_originalScale == null) { _originalScale = transform.localScale; }
            OnSequenceStart();

            canvasGroup.alpha = 0;
            var sequence = Sequence.Create(useUnscaledTime: true);
            sequence
            .Chain(Tween.Alpha(canvasGroup, 1, duration * 0.5f))
            .Group(Tween.Scale(transform, _originalScale.Value * scaleFactor, _originalScale.Value, duration, Ease.OutBack))
            .OnComplete(() => _runningSequences.Remove(sequence));

            _runningSequences.Add(sequence);
            await sequence;
        }

        /// <summary> (Fade out) + (Scale)  </summary>
        public async UniTask PlayDisappear(float duration = AnimationTime.DefaultTransitionDuration, float scaleFactor = 1.25f)
        {
            if (_originalScale == null) { _originalScale = transform.localScale; }
            OnSequenceStart();

            var sequence = Sequence.Create(useUnscaledTime: true);
            sequence
            .Chain(Tween.Alpha(canvasGroup, 0, duration * 0.95f))
            .Group(Tween.Scale(transform, _originalScale.Value * scaleFactor, duration, Ease.OutBack, useUnscaledTime: true))
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                _runningSequences.Remove(sequence);
            });

            _runningSequences.Add(sequence);
            await sequence;
        }

        public async UniTask FadeIn(float duration = AnimationTime.DefaultTransitionDuration, bool useCurrentAlpha = false)
        {
            OnSequenceStart();
            var sequence = Sequence.Create(useUnscaledTime: true);
            sequence
            .Chain(Tween.Alpha(canvasGroup, useCurrentAlpha ? canvasGroup.alpha : 0, 1, duration, useUnscaledTime: true))
            .OnComplete(() => _runningSequences.Remove(sequence));

            _runningSequences.Add(sequence);
            await sequence;
        }

        public async UniTask FadeOut(float duration = AnimationTime.DefaultTransitionDuration, bool useCurrentAlpha = true)
        {
            OnSequenceStart();
            var sequence = Sequence.Create(useUnscaledTime: true);
            sequence
            .Chain(Tween.Alpha(canvasGroup, useCurrentAlpha ? canvasGroup.alpha : 1, 0, duration, useUnscaledTime: true))
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                _runningSequences.Remove(sequence);
            });

            _runningSequences.Add(sequence);
            await sequence;
        }

        private void OnSequenceStart()
        {
            foreach (var sequence in _runningSequences)
            {
                sequence.Stop();
            }
            _runningSequences.Clear();
            gameObject.SetActive(true);
        }

    }
}