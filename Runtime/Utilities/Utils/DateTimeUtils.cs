using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DateTimeUtils
{
    public static long CurrentUnixMs => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public static long CurrentUnixSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    /// <summary> If unsigned == false, will return a negative number if timestamp is in the past </summary>
    public static long MillisecondsFromNow(long unixMs, bool unsigned = true)
    {
        var diff = unixMs - CurrentUnixMs;
        return unsigned ? (long)Mathf.Abs(diff) : diff;
    }

    /// <summary> If unsigned == false, will return a negative number if timestamp is in the past </summary>
    public static long SecondsFromNow(long unixSeconds, bool unsigned = true)
    {
        var diff = unixSeconds - CurrentUnixSeconds;
        return unsigned ? (long)Mathf.Abs(diff) : diff;
    }

    /// <summary> Replace " days", " hours", " mins", "s" with the intended suffix </summary>
    public static string ToShortestUnitString(TimeSpan timeSpan, string daySuffix = " days", string hourSuffix = " hours", string minuteSuffix = " mins", string secondSuffix = "s")
    {
        var d = (int)timeSpan.TotalDays;
        var h = timeSpan.Hours;
        var m = timeSpan.Minutes;
        var s = timeSpan.Seconds;

        if (d > 0)
        {
            if (d == 1) { daySuffix = daySuffix.Replace("days", "day"); }
            return $"{d}{daySuffix}";
        }

        if (h > 0)
        {
            if (h == 1) { hourSuffix = hourSuffix.Replace("hours", "hour"); }
            return $"{h}{hourSuffix}";
        }

        if (m > 0)
        {
            if (m == 1) { minuteSuffix = minuteSuffix.Replace("mins", "min"); }
            return $"{m}{minuteSuffix}";
        }

        return $"{s}{secondSuffix}";
    }
}
