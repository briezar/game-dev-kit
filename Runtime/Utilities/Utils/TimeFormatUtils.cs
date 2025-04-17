using System;
using UnityEngine;

public class TimeSpanFormatter : ITimeSpanFormatter
{
    private readonly Func<TimeSpan, string> _transformFunc;

    public TimeSpanFormatter(Func<TimeSpan, string> transformFunc)
    {
        _transformFunc = transformFunc;
    }

    public string Format(TimeSpan duration) => _transformFunc?.Invoke(duration) ?? duration.ToString();
}

public interface ITimeSpanFormatter
{
    string Format(TimeSpan duration);

    public static ITimeSpanFormatter Create(Func<TimeSpan, string> transformFunc) => new TimeSpanFormatter(transformFunc);

    public static readonly ITimeSpanFormatter Default = new TimeSpanFormatter(null);

    /// <summary> 1 day/days or 23h 45m or 3m 4s </summary>
    public static readonly ITimeSpanFormatter Days_HM_MS = new TimeSpanFormatter(duration =>
    {
        var d = (int)duration.TotalDays;
        var h = duration.Hours;
        var m = duration.Minutes;
        var s = duration.Seconds;

        string timeText;
        if (d > 0)
        {
            timeText = d > 1 ? $"{d} days" : $"{d} day";
        }
        else if (h > 0)
        {
            timeText = $"{h}h {m}m";
        }
        else
        {
            timeText = $"{m}m {s}s";
        }

        return timeText;
    });

    /// <summary> 1d 23h or 23h 45m or 3m 4s </summary>
    public static readonly ITimeSpanFormatter DH_HM_MS = new TimeSpanFormatter(duration =>
    {
        var d = (int)duration.TotalDays;
        var h = duration.Hours;
        var m = duration.Minutes;
        var s = duration.Seconds;

        string timeText;
        if (d > 0)
        {
            timeText = $"{d}d {h}h";
        }
        else if (h > 0)
        {
            timeText = $"{h}h {m}m";
        }
        else
        {
            timeText = $"{m}m {s}s";
        }

        return timeText;
    });

    /// <summary> 23:34:45 </summary>
    public static readonly ITimeSpanFormatter HMS = new TimeSpanFormatter(duration =>
    {
        var h = (int)duration.TotalHours;
        var m = duration.Minutes;
        var s = duration.Seconds;

        var timeText = $"{h:00}:{m:00}:{s:00}";
        return timeText;
    });

    /// <summary> 23h 34m 45s or 34m 45s </summary>
    public static readonly ITimeSpanFormatter HMS_MS = new TimeSpanFormatter(duration =>
    {
        var h = (int)duration.TotalHours;
        var m = duration.Minutes;
        var s = duration.Seconds;

        var timeText = h > 0 ? $"{h}h {m}m {s}s" : $"{m}m {s}s";
        return timeText;
    });

}

public static class TimeFormatUtils
{
    /// <inheritdoc cref="ITimeSpanFormatter.DH_HM_MS" />
    public static string DH_HM_MS(TimeSpan duration) => ITimeSpanFormatter.DH_HM_MS.Format(duration);

    /// <inheritdoc cref="ITimeSpanFormatter.HMS" />
    public static string HMS(TimeSpan duration) => ITimeSpanFormatter.HMS.Format(duration);
}