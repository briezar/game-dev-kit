using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using Cysharp.Threading.Tasks;

namespace GameDevKit.UI
{
    [Serializable]
    public class UIManagerAnimation
    {
        [SerializeField] private CanvasGroup _bgDim;
        [SerializeField] private CanvasGroup _screenFader;

        private OverlayBgDim? _popupBgDimCache;
        public OverlayBgDim PopupBgDim
        {
            get
            {
                _popupBgDimCache ??= new OverlayBgDim
                {
                    gameObject = _bgDim.gameObject,
                    canvas = _bgDim.GetComponent<Canvas>()
                };
                return _popupBgDimCache.Value;
            }
        }

        public async UniTask FadeTransition(FadeSetting fadeSetting)
        {
            if (fadeSetting.FadeInDuration != null)
            {
                await FadeScreen(1, fadeSetting.FadeInDuration.Value);
                fadeSetting.OnFadeInComplete?.Invoke();
            }

            await UniTask.WaitForSeconds(fadeSetting.WaitAfterFadeIn);

            if (fadeSetting.FadeOutCondition != null)
            {
                await UniTask.WaitUntil(fadeSetting.FadeOutCondition);
            }

            if (fadeSetting.FadeOutDuration != null)
            {
                fadeSetting.OnFadeOutStart?.Invoke();
                await FadeScreen(0, fadeSetting.FadeOutDuration.Value);
            }

            fadeSetting.OnFinish?.Invoke();
        }

        public Tween FadeScreen(float to, float duration = 0)
        {
            _screenFader.gameObject.SetActive(true);
            var tween = Tween.Alpha(_screenFader, to, duration).OnComplete(() => _screenFader.gameObject.SetActive(to > 0));
            return tween;
        }

        public Tween FadeOverlayUIDim(bool fadeIn, float duration = AnimationTime.DefaultTransitionDuration)
        {
            _bgDim.gameObject.SetActive(true);
            var tween = Tween.Alpha(_bgDim, fadeIn ? 1 : 0, duration).OnComplete(() => _bgDim.gameObject.SetActive(fadeIn));
            return tween;
        }

    }
}