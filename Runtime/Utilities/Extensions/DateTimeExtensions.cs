using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class DateTimeExtensions
{
    public static long ToUnixTimeSeconds(this DateTime date)
    {
        var dto = (DateTimeOffset)date;
        return dto.ToUnixTimeSeconds();
    }

    public static long ToUnixTimeMilliseconds(this DateTime date)
    {
        var dto = (DateTimeOffset)date;
        return dto.ToUnixTimeMilliseconds();
    }
}