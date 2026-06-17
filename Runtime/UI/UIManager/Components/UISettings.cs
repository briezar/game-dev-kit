using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit.UI
{
    [Flags]
    public enum ShowOverlayBehaviour
    {
        None = 0,
        DimLowerUI = 1 << 0,
    }

    public struct FadeSetting
    {
        public const float DefaultFadeDuration = 0.3f;

        public float? FadeInDuration;
        public float? FadeOutDuration;
        public float WaitAfterFadeIn;
        public Action OnFadeInComplete;
        public Action OnFadeOutStart;
        public Action OnFinish;
        public Func<bool> FadeOutCondition;


        /// <summary> Fade to black </summary>
        public static FadeSetting FadeIn(float fadeInDuration = DefaultFadeDuration, float waitAfterFadeIn = 0)
        {
            var fadeSetting = new FadeSetting()
            {
                FadeInDuration = fadeInDuration,
                WaitAfterFadeIn = waitAfterFadeIn
            };
            return fadeSetting;
        }

        /// <summary> Fade to transparent </summary>
        public static FadeSetting FadeOut(float fadeOutDuration = DefaultFadeDuration)
        {
            var fadeSetting = new FadeSetting()
            {
                FadeOutDuration = fadeOutDuration
            };
            return fadeSetting;
        }

        public static FadeSetting FadeInOut(float fadeInDuration = DefaultFadeDuration, float fadeOutDuration = DefaultFadeDuration, float waitAfterFadeIn = 0)
        {
            var fadeSetting = new FadeSetting()
            {
                FadeInDuration = fadeInDuration,
                FadeOutDuration = fadeOutDuration,
                WaitAfterFadeIn = waitAfterFadeIn
            };
            return fadeSetting;
        }
    }
}