using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoroutineUtils
{
    /// <summary>
    /// Starts a countdown timer on a TMP_Text component using the specified <paramref name="timeFormatter"/>  and <paramref name="extraFormat"/> for string representation
    /// </summary>
    /// <param name="extraFormat">null for no format, else provide text around "{0}". E.g. "Time Left: {0}"</param>
    public static IEnumerator TimerRoutine(TMP_Text timerText, DateTimeOffset timeEnd, ITimeSpanFormatter timeFormatter, Action onTimerEnd = null, string extraFormat = null)
    {
        timeFormatter ??= ITimeSpanFormatter.Default;
        return TimerRoutine(timerText, timeEnd, timeFormatter.Format, onTimerEnd, extraFormat);
    }

    /// <summary>
    /// Starts a countdown timer on a TMP_Text component using the specified <paramref name="timeFormatter"/>  and <paramref name="extraFormat"/> for string representation
    /// </summary>
    /// <param name="extraFormat">null for no format, else provide text around "{0}". E.g. "Time Left: {0}"</param>
    public static IEnumerator TimerRoutine(TMP_Text timerText, DateTimeOffset timeEnd, Func<TimeSpan, string> timeFormatter, Action onTimerEnd = null, string extraFormat = null)
    {
        timeFormatter ??= ITimeSpanFormatter.Default.Format;
        var timeRemain = timeEnd - DateTimeOffset.UtcNow;

        UpdateTimerText();
        var lastSecond = timeRemain.Seconds;

        while (timeRemain.Ticks > 0)
        {
            yield return null;
            timeRemain = timeEnd - DateTimeOffset.UtcNow;
            if (timeRemain.Seconds == lastSecond) { continue; }
            lastSecond = timeRemain.Seconds;

            UpdateTimerText();
        }
        onTimerEnd?.Invoke();


        void UpdateTimerText()
        {
            var timeFormat = timeFormatter == null ? timeRemain.ToString() : timeFormatter(timeRemain);
            timerText.text = extraFormat == null ? timeFormat : extraFormat.Format(timeFormat);
        }
    }
}
