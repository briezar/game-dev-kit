using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace GameDevKit.UI
{
    public static class UIAnimationInstruction
    {
        [Serializable]
        public abstract class Instruction
        {
            public Transform transform { get; private set; }
            public RectTransform rectTransform => (RectTransform)transform;
            public GameObject gameObject => transform.gameObject;

            public float Duration = AnimationTime.DefaultTransitionDuration;

            private CanvasGroup _cacheCanvasGroup;
            protected CanvasGroup canvasGroup => _cacheCanvasGroup ??= transform.GetOrAddComponent<CanvasGroup>();

            public void SetTransform(Transform target)
            {
                transform = target;
                OnSetTransform();
            }

            protected virtual void OnSetTransform() { }

            public abstract UniTask Play();
        }

        [Serializable]
        public class FadeInScaleUp : Instruction
        {
            public float ScaleFactor = 1.5f;

            public Vector3 OriginalScale { get; private set; }

            protected override void OnSetTransform() => OriginalScale = transform.localScale;

            public override async UniTask Play()
            {
                gameObject.SetActive(true);

                canvasGroup.alpha = 0;
                var sequence = Sequence.Create(useUnscaledTime: true)
                .Chain(Tween.Alpha(canvasGroup, 0, 1, Duration * 0.5f))
                .Group(Tween.Scale(transform, OriginalScale * ScaleFactor, OriginalScale, Duration, Ease.OutBack));

                await sequence;
            }
        }

        [Serializable]
        public class FadeOutScaleUp : Instruction
        {
            public float ScaleFactor = 1.25f;

            public Vector3 OriginalScale { get; private set; }

            protected override void OnSetTransform() => OriginalScale = transform.localScale;

            public override async UniTask Play()
            {
                gameObject.SetActive(true);

                canvasGroup.alpha = 0;
                var sequence = Sequence.Create(useUnscaledTime: true)
                .Chain(Tween.Alpha(canvasGroup, 1, 0, Duration * 0.95f))
                .Group(Tween.Scale(transform, OriginalScale, OriginalScale * ScaleFactor, Duration));

                await sequence;
            }
        }

        [Serializable]
        public class FadeIn : Instruction
        {
            public override async UniTask Play()
            {
                gameObject.SetActive(true);

                var sequence = Sequence.Create(useUnscaledTime: true)
                .Chain(Tween.Alpha(canvasGroup, 0, 1, Duration, useUnscaledTime: true));

                await sequence;
            }
        }

        [Serializable]
        public class FadeOut : Instruction
        {
            public override async UniTask Play()
            {
                gameObject.SetActive(true);

                var sequence = Sequence.Create(useUnscaledTime: true)
                .Chain(Tween.Alpha(canvasGroup, 1, 0, Duration, useUnscaledTime: true));

                await sequence;
            }
        }

        public abstract class SlideInstruction : Instruction
        {
            public enum SlideDirection { None, LeftToRight, RightToLeft }

            public Vector3 OriginalAnchoredPos { get; private set; }

            protected float _totalOffsetX;

            protected override void OnSetTransform()
            {
                OriginalAnchoredPos = rectTransform.anchoredPosition;

                var canvas = rectTransform.GetComponentInParent<Canvas>().rootCanvas;
                var canvasRect = (RectTransform)canvas.transform;

                var maxExtent = Mathf.Max(-rectTransform.rect.xMin, rectTransform.rect.xMax);
                _totalOffsetX = maxExtent + canvasRect.rect.width;
            }
        }

        [Serializable]
        public class SlideIn : SlideInstruction
        {
            public SlideDirection Direction = SlideDirection.LeftToRight;
            public bool FadeIn = true;

            public override async UniTask Play()
            {
                gameObject.SetActive(true);

                var sequence = Sequence.Create(useUnscaledTime: true);

                if (Direction is not SlideDirection.None)
                {
                    var xOffset = _totalOffsetX * (Direction is SlideDirection.LeftToRight ? -1 : 1);
                    sequence.Chain(Tween.UIAnchoredPositionX(rectTransform, OriginalAnchoredPos.x + xOffset, OriginalAnchoredPos.x, Duration));
                }

                if (FadeIn)
                {
                    sequence.Group(Tween.Alpha(canvasGroup, 0, 1, Duration, useUnscaledTime: true));
                }
            }
        }

        [Serializable]
        public class SlideOut : SlideInstruction
        {
            public SlideDirection Direction = SlideDirection.LeftToRight;
            public bool FadeOut = true;

            public override async UniTask Play()
            {
                gameObject.SetActive(true);

                var sequence = Sequence.Create(useUnscaledTime: true);

                if (Direction is not SlideDirection.None)
                {
                    var xOffset = _totalOffsetX * (Direction is SlideDirection.LeftToRight ? 1 : -1);
                    sequence.Chain(Tween.UIAnchoredPositionX(rectTransform, OriginalAnchoredPos.x, OriginalAnchoredPos.x + xOffset, Duration, Ease.InQuad));
                }

                if (FadeOut)
                {
                    sequence.Group(Tween.Alpha(canvasGroup, 1, 0, Duration, useUnscaledTime: true));
                }
            }
        }

    }
}