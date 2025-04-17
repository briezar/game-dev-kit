using System;
using UnityEngine;

namespace GameDevKit.UI
{
    public class UIAnimInstruction : MonoBehaviour
    {
        private class ArgsBase { public float duration = AnimationTime.DefaultTransitionDuration; }

        [Serializable]
        private class PlayAppearArgs : ArgsBase { public float scaleFactor = 1.5f; }

        [Serializable]
        private class PlayDisappearArgs : ArgsBase { public float scaleFactor = 1.25f; }

        [Serializable]
        private class FadeInArgs : ArgsBase
        {
            public float scaleFactor = 1.25f;
            public bool useCurrentAlpha = false;
        }

        [Serializable]
        private class FadeOutArgs : ArgsBase
        {
            public float scaleFactor = 1.25f;
            public bool useCurrentAlpha = true;
        }

        [SerializeField] private PlayAppearArgs _playAppearArgs;
        [SerializeField] private PlayDisappearArgs _playDisappearArgs;
        [SerializeField] private FadeInArgs _fadeInArgs;
        [SerializeField] private FadeOutArgs _fadeOutArgs;

        private UIAnim _cacheUIanim;
        public UIAnim Anim => _cacheUIanim != null ? _cacheUIanim : _cacheUIanim = this.GetOrAddComponent<UIAnim>();

        public void PlayAppear()
        {
            Anim.PlayAppear(_playAppearArgs.duration, _playAppearArgs.scaleFactor);
        }

        public void PlayDisappear()
        {
            Anim.PlayDisappear(_playDisappearArgs.duration, _playDisappearArgs.scaleFactor);
        }

        public void FadeIn()
        {
            Anim.FadeIn(_fadeInArgs.duration, _fadeInArgs.useCurrentAlpha);
        }

        public void FadeOut()
        {
            Anim.FadeOut(_fadeOutArgs.duration, _fadeOutArgs.useCurrentAlpha);
        }
    }
}