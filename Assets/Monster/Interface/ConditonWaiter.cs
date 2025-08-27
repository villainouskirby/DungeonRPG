using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class ConditionAwaiter
{
    /// <summary>
    /// predicate 가 seconds 동안 "연속으로" 참이면 true 반환.
    /// 중간에 거짓이 되는 순간 경과시간을 0으로 리셋한다.
    /// onProgress(t) 로 0~1 진행도 콜백 가능(선택).
    /// </summary>
    public static async UniTask<bool> HoldTrueContinuously(
        float seconds,
        Func<bool> predicate,
        Action<float> onProgress,
        CancellationToken token)
    {
        float elapsed = 0f;
        seconds = Mathf.Max(0.01f, seconds);

        while (!token.IsCancellationRequested)
        {
            if (predicate())
            {
                elapsed += Time.deltaTime;
                onProgress?.Invoke(Mathf.Clamp01(elapsed / seconds));
                if (elapsed >= seconds) return true;
            }
            else
            {
                elapsed = 0f;
                onProgress?.Invoke(0f);
            }
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        return false;
    }

    /// <summary>
    /// predicate 가 seconds 동안 "연속으로" 거짓이면 true 반환.
    /// 중간에 참이 되는 순간 경과시간을 0으로 리셋한다.
    /// </summary>
    public static async UniTask<bool> HoldFalseContinuously(
        float seconds,
        Func<bool> predicate,
        Action<float> onProgress,
        CancellationToken token)
    {
        float elapsed = 0f;
        seconds = Mathf.Max(0.01f, seconds);

        while (!token.IsCancellationRequested)
        {
            if (!predicate())
            {
                elapsed += Time.deltaTime;
                onProgress?.Invoke(Mathf.Clamp01(elapsed / seconds));
                if (elapsed >= seconds) return true;
            }
            else
            {
                elapsed = 0f;
                onProgress?.Invoke(0f);
            }
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        return false;
    }
}