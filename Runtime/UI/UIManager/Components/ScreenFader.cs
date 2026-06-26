using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace GameDevKit.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;

        public Canvas canvas => _canvas;
        public CanvasGroup canvasGroup => _canvasGroup;

        private void Reset()
        {
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
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

        public Tween FadeScreen(float normalizedValue, float duration = 0)
        {
            gameObject.SetActive(true);
            var tween = Tween.Alpha(canvasGroup, normalizedValue, duration).OnComplete(() => gameObject.SetActive(normalizedValue > 0));
            return tween;
        }
    }
}