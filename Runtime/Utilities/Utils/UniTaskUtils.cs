using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class UniTaskUtils
{
    /// <summary> Waits for a specified duration or indefinitely if the duration is null or negative. </summary>
    public static UniTask WaitForSecondsOrIndefinitely(float? duration, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default, bool cancelImmediately = false)
    {
        return duration == null || duration < 0
                ? UniTask.Never(cancellationToken)
                : UniTask.WaitForSeconds(duration.Value, ignoreTimeScale, delayTiming, cancellationToken, cancelImmediately);
    }

    public static CancellationTokenSource GetTimeoutTokenSource(TimeSpan delay)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfterSlim(delay);
        return cts;
    }
    public static CancellationToken GetTimeoutToken(TimeSpan delay) => GetTimeoutTokenSource(delay).Token;
    public static CancellationToken GetTimeoutToken(float seconds) => GetTimeoutTokenSource(TimeSpan.FromSeconds(seconds)).Token;

    public static UniTask WaitForProgress(float durationSeconds, Action<float> onProgress, float? callbackIntervalSeconds = null, bool ignoreTimeScale = true, CancellationToken token = default)
        => WaitForProgress(TimeSpan.FromSeconds(durationSeconds), onProgress, callbackIntervalSeconds.HasValue ? TimeSpan.FromSeconds(callbackIntervalSeconds.Value) : null, ignoreTimeScale, token);
    public static async UniTask WaitForProgress(TimeSpan duration, Action<float> onProgress, TimeSpan? callbackInterval = null, bool ignoreTimeScale = true, CancellationToken token = default)
    {
        var elapsed = TimeSpan.Zero;
        while (elapsed < duration && !token.IsCancellationRequested)
        {
            onProgress?.Invoke((float)(elapsed / duration));
            if (callbackInterval == null)
            {
                await UniTask.Yield(token).SuppressCancellationThrow();
                elapsed += TimeSpan.FromSeconds(ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
            }
            else
            {
                await UniTask.Delay(callbackInterval.Value, ignoreTimeScale, cancellationToken: token).SuppressCancellationThrow(); ;
                elapsed += callbackInterval.Value;
            }
        }

        onProgress?.Invoke(1f);
    }
    /// <summary>
    /// Wait until the FPS is stable, meaning the frame rate is close to the target frame rate.
    /// This is useful to ensure that the game is not lagging before proceeding with certain operations.
    /// The default threshold is 80% of the target frame rate.
    /// For example, if the target frame rate is 60 FPS, it will wait until the FPS is at least 48 FPS.
    /// </summary>
    public static async UniTask WaitUntilStableFps(float threshold = 0.8f, CancellationToken token = default, float maxTime = 1f)
    {
        var startTime = Time.realtimeSinceStartup;
        while (!token.IsCancellationRequested)
        {
            var targetFps = Application.targetFrameRate;
            if (targetFps <= 0) { targetFps = 30; } // iOS & Android default target frame rate is 30, it is also a reasonable fallback value

            var fps = Time.smoothDeltaTime > 0f ? (1f / Time.smoothDeltaTime) : 0f;
            if (fps >= targetFps * threshold) { break; }
            await UniTask.Yield();

            if (Time.realtimeSinceStartup - startTime > maxTime)
            {
                Debug.LogWarning($"WaitUntilStableFPS timed out after {maxTime} seconds. Current FPS: {fps}, Target FPS: {targetFps}");
                break;
            }
        }
    }

    public static async UniTask WaitForAnyKey(CancellationToken token = default)
    {
        while (!Input.anyKeyDown && !token.IsCancellationRequested)
        {
            await UniTask.Yield();
        }
    }

    public static CancellationToken GetCancellationTokenOnDisable(this Component component) => component.gameObject.GetCancellationTokenOnDisable();
    public static CancellationToken GetCancellationTokenOnDisable(this GameObject gameObject) => gameObject.GetOrAddComponent<AsyncDisableTrigger>().CancellationToken;

    [DisallowMultipleComponent]
    private sealed class AsyncDisableTrigger : MonoBehaviour
    {
        CancellationTokenSource _cts;

        public CancellationToken CancellationToken
        {
            get
            {
                _cts ??= new CancellationTokenSource();
                return _cts.Token;
            }
        }

        private void OnEnable()
        {
            _cts ??= new CancellationTokenSource();
        }

        private void OnDisable()
        {
            Cancel();
        }

        private void Cancel()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}