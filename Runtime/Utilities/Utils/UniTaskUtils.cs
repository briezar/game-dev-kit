using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class UniTaskUtils
{
    public static async UniTask WaitUntilStableFPS(float threshold = 0.8f)
    {
        while ((1 / Time.smoothDeltaTime) < (Application.targetFrameRate * threshold))
        {
            await UniTask.Yield();
        }
    }

    /// <summary> Waits for a specified duration or indefinitely if the duration is negative. </summary>
    public static UniTask WaitForSecondsOrIndefinitely(float duration, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, CancellationToken cancellationToken = default, bool cancelImmediately = false)
    {
        return duration < 0
                ? UniTask.Never(cancellationToken)
                : UniTask.WaitForSeconds(duration, ignoreTimeScale, delayTiming, cancellationToken, cancelImmediately);
    }

    public static CancellationToken GetCancellationTokenOnDisable(this Component component)
    {
        return component.gameObject.GetCancellationTokenOnDisable();
    }

    public static CancellationToken GetCancellationTokenOnDisable(this GameObject gameObject)
    {
        return gameObject.GetOrAddComponent<AsyncDisableTrigger>().CancellationToken;
    }

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