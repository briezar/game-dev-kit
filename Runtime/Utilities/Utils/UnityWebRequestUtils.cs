using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class UnityWebRequestUtils
{
    public static async UniTask<long> GetDownloadSize(string url)
    {
        using var webRequest = UnityWebRequest.Head(url);
        try
        {
            await webRequest.SendWebRequest();
            var sizeString = webRequest.GetResponseHeader("Content-Length");
            if (sizeString == null)
            {
                Debug.LogWarning($"Cannot find Content-Length header for url: {url}");
                return -1;
            }
            return sizeString.ToLong();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting download size from url [{url}]:  {ex}");
            return -1;
        }
    }
}