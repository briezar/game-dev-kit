using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class StringExtensions
{
    public static bool OrdinalEquals(this string text, string other, bool ignoreCase = false) => string.Equals(text, other, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    /// <summary> Same results as <see cref="OrdinalEquals"/>. Speed depends on the runtime (Mono Editor vs IL2CPP build), so benchmark on your target. </summary>
    public static bool FastOrdinalEquals(this string text, string other)
    {
        if (ReferenceEquals(text, other)) { return true; }
        if (text is null || other is null || text.Length != other.Length) { return false; }
        return string.CompareOrdinal(text, other) == 0;
    }

    /// <summary> Trims <paramref name="text"/> in place. Returns false if the result is null or empty. </summary>
    private static bool TryTrim(ref string text)
    {
        text = text?.Trim();
        return !text.IsNullOrEmpty();
    }

    public static bool ToBool(this string text)
    {
        if (!TryTrim(ref text)) { return default; }
        if (!bool.TryParse(text, out var result))
        {
            Debug.LogWarning("Invalid string to bool format: " + text);
        }
        return result;
    }
    public static int ToInt(this string text, IFormatProvider provider = null)
    {
        if (!TryTrim(ref text)) { return default; }
        if (!int.TryParse(text, NumberStyles.Integer, provider ?? CultureInfo.InvariantCulture, out var result))
        {
            Debug.LogWarning("Invalid string to int format: " + text);
        }
        return result;
    }
    public static float ToFloat(this string text, IFormatProvider provider = null)
    {
        if (!TryTrim(ref text)) { return default; }
        if (!float.TryParse(text, NumberStyles.Float, provider ?? CultureInfo.InvariantCulture, out var result))
        {
            Debug.LogWarning("Invalid string to float format: " + text);
        }
        return result;
    }
    public static long ToLong(this string text, IFormatProvider provider = null)
    {
        if (!TryTrim(ref text)) { return default; }
        if (!long.TryParse(text, NumberStyles.Integer, provider ?? CultureInfo.InvariantCulture, out var result))
        {
            Debug.LogWarning("Invalid string to long format: " + text);
        }
        return result;
    }

    private static class EnumInfo<T> where T : struct, Enum
    {
        public static readonly bool IsFlags = typeof(T).IsDefined(typeof(FlagsAttribute), false);
    }

    /// <summary> Parses by name or number. Numbers that match no member are rejected, except for [Flags] enums. </summary>
    public static T ToEnum<T>(this string text, bool ignoreCase = true) where T : struct, Enum
    {
        if (!TryTrim(ref text)) { return default; }
        if (!Enum.TryParse(text, ignoreCase, out T result) || (!EnumInfo<T>.IsFlags && !Enum.IsDefined(typeof(T), result)))
        {
            Debug.LogWarning($"Cannot parse string to {typeof(T)}: {text}");
            return default;
        }
        return result;
    }

    public static Vector2 ToVector2(this string text) => text.ToVector3();
    public static Vector3 ToVector3(this string text)
    {
        if (!TryTrim(ref text)) { return default; }

        var split = text.Trim('(', ')').Split(',');
        var max = Mathf.Min(split.Length, 3);
        var result = Vector3.zero;
        for (int i = 0; i < max; i++)
        {
            result[i] = split[i].ToFloat();
        }
        return result;
    }

    public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);

    public static string Format(this string text, object arg0) => string.Format(text, arg0);
    public static string Format(this string text, params object[] args) => string.Format(text, args);

    private static readonly Regex _controlCharsRegex = new(@"\p{Cc}+", RegexOptions.Compiled, TimeSpan.FromSeconds(1.5));
    private static readonly Regex _controlCharsKeepWhitespaceRegex = new(@"[\p{Cc}-[\t\r\n]]+", RegexOptions.Compiled, TimeSpan.FromSeconds(1.5));

    /// <summary>
    /// Removes control characters (U+0000-001F, U+007F-009F). Format characters (zero-width joiners) and surrogate pairs (emoji) are kept.
    /// If <paramref name="keepWhitespace"/> is true, tab, CR and LF are kept too.
    /// </summary>
    public static string CleanInput(this string input, bool keepWhitespace = false)
    {
        if (input.IsNullOrEmpty()) { return input; }
        try
        {
            return (keepWhitespace ? _controlCharsKeepWhitespaceRegex : _controlCharsRegex).Replace(input, string.Empty);
        }
        // If timeout when replacing invalid characters, return Empty.
        catch (RegexMatchTimeoutException)
        {
            return string.Empty;
        }
    }

    /// <summary> Reverses by text element, so surrogate pairs and combining marks stay intact. </summary>
    public static string Reverse(this string input)
    {
        if (input.IsNullOrEmpty() || input.Length == 1) { return input; }

        var elementStarts = StringInfo.ParseCombiningCharacters(input);
        return string.Create(input.Length, (input, elementStarts), static (chars, state) =>
        {
            var (text, starts) = state;
            var position = 0;
            for (var i = starts.Length - 1; i >= 0; i--)
            {
                var start = starts[i];
                var end = i == starts.Length - 1 ? text.Length : starts[i + 1];
                var length = end - start;
                text.AsSpan(start, length).CopyTo(chars.Slice(position));
                position += length;
            }
        });
    }

    public static string ToHex(this string input, Encoding encoding = null)
    {
        if (input.IsNullOrEmpty()) { return string.Empty; }
        encoding ??= Encoding.UTF8;
        return BitConverter.ToString(encoding.GetBytes(input)).Replace("-", "");
    }

    /// <summary> Returns an empty string (and logs a warning) if the input is not valid hex. </summary>
    public static string FromHex(this string hexString, Encoding encoding = null)
    {
        if (hexString.IsNullOrEmpty()) { return string.Empty; }
        hexString = hexString.Replace("-", "");
        if (hexString.Length % 2 != 0)
        {
            Debug.LogWarning($"Hex string must have an even length: {hexString}");
            return string.Empty;
        }

        var bytes = new byte[hexString.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            if (!byte.TryParse(hexString.AsSpan(i * 2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out bytes[i]))
            {
                Debug.LogWarning($"Invalid hex string: {hexString}");
                return string.Empty;
            }
        }
        return (encoding ?? Encoding.UTF8).GetString(bytes);
    }

    public static char GetLastChar(this string text) => text.IsNullOrEmpty() ? '\0' : text[^1];

    /// <summary> Substring from <paramref name="startIndex"/> (inclusive) to <paramref name="endIndex"/> (exclusive). Returns an empty string for an invalid range. </summary>
    public static string GetBetween(this string text, int startIndex, int endIndex)
    {
        if (text.IsNullOrEmpty()) { return string.Empty; }
        if (startIndex < 0 || endIndex > text.Length || startIndex > endIndex)
        {
            Debug.LogWarning($"Invalid range [{startIndex}..{endIndex}) for string of length {text.Length}.");
            return string.Empty;
        }
        return text[startIndex..endIndex];
    }

    public static string RemoveFirst(this string input, string toRemove)
    {
        if (input.IsNullOrEmpty() || toRemove.IsNullOrEmpty()) { return input; }
        var index = input.IndexOf(toRemove, StringComparison.Ordinal);
        if (index < 0) { return input; }
        return input.Remove(index, toRemove.Length);
    }

    public static string RemoveLast(this string input, string toRemove)
    {
        if (input.IsNullOrEmpty() || toRemove.IsNullOrEmpty()) { return input; }
        var index = input.LastIndexOf(toRemove, StringComparison.Ordinal);
        if (index < 0) { return input; }
        return input.Remove(index, toRemove.Length);
    }

    public static string RemoveFromEnd(this string input, string toRemove)
    {
        if (input.IsNullOrEmpty() || toRemove.IsNullOrEmpty()) { return input; }
        return input.EndsWith(toRemove, StringComparison.Ordinal) ? input[..^toRemove.Length] : input;
    }

    // Boundaries: lower->Upper, ACRONYMWord, letter->digit, digit->letter. All matches are zero-width.
    private static readonly Regex _separateCamelCaseRegex = new(@"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|(?<=[A-Za-z])(?=\d)|(?<=\d)(?=[A-Za-z])", RegexOptions.Compiled);

    /// <summary> "SeparateCamelCase" -> "Separate Camel Case", "HTTPServer2" -> "HTTP Server 2" </summary>
    public static string SeparateCamelCase(this string text, string separator = " ")
    {
        if (text == null || text.Length <= 1) { return text; }
        // '$' is special in regex replacement patterns.
        return _separateCamelCaseRegex.Replace(text, (separator ?? string.Empty).Replace("$", "$$"));
    }

    // TMP parses rich text numbers with a '.' decimal separator, so never use the current culture here.
    private static string ToTag(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    public static string VOffset(this string text, float offset) => $"<voffset={ToTag(offset)}em>{text}</voffset>";
    public static string Bold(this string text) => $"<b>{text}</b>";
    public static string Colorize(this string text, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>";
    public static string Colorize(this string text, string hex) => $"<color=#{hex.TrimStart('#')}>{text}</color>";
    public static string Resize(this string text, float scale) => $"<size={ToTag(scale * 100f)}%>{text}</size>";
    public static string NoBreak(this string text) => $"<nobr>{text}</nobr>";
}