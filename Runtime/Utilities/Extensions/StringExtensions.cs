using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Globalization;

public static class StringExtensions
{
    public static bool OrdinalEquals(this string text, string other, bool ignoreCase = false) => string.Equals(text, other, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    /// <summary> Pitfalls are unknown! But it is faster than <see cref="OrdinalEquals"/> </summary>
    public static bool FastOrdinalEquals(this string text, string other)
    {
        if (text is null && other is null) { return true; }
        if (text is null || other is null || text.Length != other.Length) { return false; }
        return string.CompareOrdinal(text, other) == 0;
    }

    private static bool IsParsable(ref string text)
    {
        text = text?.Trim();
        return !text.IsNullOrEmpty();
    }

    public static bool ToBool(this string text)
    {
        if (!IsParsable(ref text)) { return default; }
        if (!bool.TryParse(text, out var result))
        {
            Debug.LogWarning("Invalid string to bool format: " + text);
        }
        return result;
    }
    public static int ToInt(this string text, IFormatProvider provider = null)
    {
        if (!IsParsable(ref text)) { return default; }
        if (!int.TryParse(text, NumberStyles.Integer, provider ?? CultureInfo.InvariantCulture, out var result))
        {
            Debug.LogWarning("Invalid string to int format: " + text);
        }
        return result;
    }
    public static float ToFloat(this string text, IFormatProvider provider = null)
    {
        if (!IsParsable(ref text)) { return default; }
        if (!float.TryParse(text, NumberStyles.Float, provider ?? CultureInfo.InvariantCulture, out var result))
        {
            Debug.LogWarning("Invalid string to float format: " + text);
        }
        return result;
    }
    public static long ToLong(this string text, IFormatProvider provider = null)
    {
        if (!IsParsable(ref text)) { return default; }
        if (!long.TryParse(text, NumberStyles.Integer, provider ?? CultureInfo.InvariantCulture, out var result))
        {
            Debug.LogWarning("Invalid string to long format: " + text);
        }
        return result;
    }
    public static T ToEnum<T>(this string text, bool ignoreCase = true) where T : struct, Enum
    {
        text = text?.Trim();
        if (text.IsNullOrEmpty()) { return default; }
        if (!Enum.TryParse(text, ignoreCase, out T result))
        {
            Debug.LogWarning($"Cannot parse string to {typeof(T)}: {text}");
        }
        return result;
    }

    public static Vector2 ToVector2(this string text) => ToVector3(text);
    public static Vector3 ToVector3(this string text)
    {
        if (!IsParsable(ref text)) { return default; }
        var result = Vector3.zero;

        try
        {
            var split = text.Replace("(", "").Replace(")", "").Split(',');
            var max = Mathf.Min(split.Length, 3);
            for (int i = 0; i < max; i++)
            {
                result[i] = split[i].ToFloat();
            }
        }
        catch (Exception)
        {
            Debug.LogWarning($"Cannot parse string to Vector3: {text}");
        }
        return result;
    }

    public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);

    public static string Format(this string text, object arg0) => string.Format(text, arg0);
    public static string Format(this string text, params object[] args) => string.Format(text, args);

    public static string CleanInput(this string input)
    {
        // Replace invalid characters with empty strings.
        try
        {
            return Regex.Replace(input, @"\p{C}+", "", RegexOptions.None, TimeSpan.FromSeconds(1.5));
        }
        // If timeout when replacing invalid characters, return Empty.
        catch (RegexMatchTimeoutException)
        {
            return string.Empty;
        }
    }

    public static string Reverse(this string input)
    {
        var reversedString = string.Create(input.Length, input, (chars, state) =>
        {
            state.AsSpan().CopyTo(chars);
            chars.Reverse();
        });
        return reversedString;
    }

    public static string ToHex(this string input, Encoding encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = encoding.GetBytes(input);
        var hexString = BitConverter.ToString(bytes).Replace("-", "");
        return hexString;
    }

    public static string FromHex(this string hexString, Encoding encoding = null)
    {
        encoding ??= Encoding.UTF8;
        hexString = hexString.Replace("-", "");
        var bytes = new byte[hexString.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        }
        return encoding.GetString(bytes);
    }

    public static char GetLastChar(this string text) => text.IsNullOrEmpty() ? '\0' : text[^1];

    public static string GetBetween(this string text, int startIndex, int endIndex)
    {
        if (startIndex > text.Length || endIndex > text.Length)
        {
            Debug.LogWarning($"Index exceeds string length!");
            return text;
        }
        return text[startIndex..endIndex];
    }

    private static readonly Regex _separateCamelCaseRegex = new(@"(?<=[a-z])(?=[A-Z])|(?<=[A-Za-z])(?=\d)|(?<=\d)(?=[A-Za-z])", RegexOptions.Compiled);

    /// <summary> "SeparateCamelCase" -> "Separate Camel Case" </summary>
    public static string SeparateCamelCase(this string text, string separator = " ") => (text == null || text.Length <= 1) ? text : _separateCamelCaseRegex.Replace(text, separator + "$1");

    public static string VOffset(this string text, float offset) => $"<voffset={offset}em>{text}</voffset>";
    public static string Bold(this string text) => $"<b>{text}</b>";
    public static string Colorize(this string text, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>";
    public static string Colorize(this string text, string hex) => $"<color=#{hex.TrimStart('#')}>{text}</color>";
    public static string Resize(this string text, float scale) => $"<size={scale * 100}%>{text}</size>";
    public static string NoBreak(this string text) => $"<nobr>{text}</nobr>";

}
