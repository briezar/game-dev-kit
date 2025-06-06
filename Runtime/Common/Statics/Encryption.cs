using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Encryption
{
    private static readonly string XOR_Key = "Encryption".ToHex().ToHex();

    // Basic encryption method, much faster than AES and is enough for local data encryption
    public static string XOR(string data, string key = null)
    {
        if (data.IsNullOrEmpty()) { return string.Empty; }
        key ??= XOR_Key;

        var dataLength = data.Length;
        var keyLength = key.Length;
        var output = data.ToCharArray();

        for (int i = 0; i < dataLength; ++i)
        {
            output[i] = (char)(data[i] ^ key[i % keyLength]);
        }

        return new string(output);
    }
}