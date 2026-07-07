using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Internet
{
    public const long Kilobyte = 1024;

    public enum DataUnit { Byte = 0, KB = 1, MB = 2, GB = 3 }

    public static string GetShortenedSize(long dataSizeInBytes)
    {
        // 2^3=8, Mathf.Log(8, 2) = 3
        // KB^2=MB, KB^3=GB
        var dataUnit = (int)Mathf.Log(dataSizeInBytes, Kilobyte).ClampMin(1);
        var scaledDataUnit = Mathf.Pow(Kilobyte, dataUnit);
        var shortenedSize = dataSizeInBytes / scaledDataUnit;

        return $"{shortenedSize:F2} {(DataUnit)dataUnit}";
    }

    public static bool IsConnected => (Application.internetReachability != NetworkReachability.NotReachable);
}
