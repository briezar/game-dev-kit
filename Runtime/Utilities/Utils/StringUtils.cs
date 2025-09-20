
using System;
using System.Globalization;
using System.Text;
using UnityEngine.Pool;

public static class StringUtils
{
    /// <summary>
    /// Strip rich text style tags (e.g. &lt;color&gt;) and decode common entities (lt, gt, amp, quot, and numeric)<br/>
    /// Handles quoted attributes (won't treat '>' inside quotes as tag end).
    /// </summary>
    public static string StripRichTextTags(string input)
    {
        if (string.IsNullOrEmpty(input)) { return string.Empty; }

        using var _ = StringBuilderPool.Get(out var sb);
        int len = input.Length;
        int i = 0;

        while (i < len)
        {
            char c = input[i];

            // Tag start: skip until the next '>' that is NOT inside quotes.
            if (c == '<')
            {
                i++; // move past '<'
                bool inDoubleQuote = false;
                bool inSingleQuote = false;

                while (i < len)
                {
                    char tc = input[i];

                    // toggle quote state (ignore quotes if inside the other quote type)
                    if (tc == '"' && !inSingleQuote) inDoubleQuote = !inDoubleQuote;
                    else if (tc == '\'' && !inDoubleQuote) inSingleQuote = !inSingleQuote;
                    else if (tc == '>' && !inDoubleQuote && !inSingleQuote)
                    {
                        i++; // consume the '>'
                        break;
                    }

                    i++;
                }

                // continue outer loop from first char after the tag (or end of string)
                continue;
            }

            // Entity handling (outside tags)
            if (c == '&')
            {
                int semi = -1;
                int maxLook = Math.Min(len, i + 16); // safe cap for entity length
                for (int j = i + 1; j < maxLook; j++)
                {
                    char cc = input[j];
                    if (cc == ';') { semi = j; break; }
                    if (char.IsWhiteSpace(cc) || cc == '<' || cc == '>') break; // not a valid entity
                }

                if (semi != -1)
                {
                    string entity = input.Substring(i + 1, semi - (i + 1));

                    // Named entities
                    if (string.Equals(entity, "lt", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append('<'); i = semi + 1; continue;
                    }
                    if (string.Equals(entity, "gt", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append('>'); i = semi + 1; continue;
                    }
                    if (string.Equals(entity, "amp", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append('&'); i = semi + 1; continue;
                    }
                    if (string.Equals(entity, "quot", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append('"'); i = semi + 1; continue;
                    }

                    // Numeric entities: &#123; or &#x1F600;
                    if (entity.Length > 0 && entity[0] == '#')
                    {
                        int codePoint;
                        bool parsed = false;
                        if (entity.Length > 1 && (entity[1] == 'x' || entity[1] == 'X'))
                        {
                            parsed = int.TryParse(entity.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint);
                        }
                        else
                        {
                            parsed = int.TryParse(entity.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out codePoint);
                        }

                        if (parsed)
                        {
                            sb.Append(CodePointToString(codePoint));
                            i = semi + 1;
                            continue;
                        }
                    }

                    // Unrecognized entity -> fallthrough and append literal '&'
                }

                // couldn't parse an entity: append literal '&'
                sb.Append('&');
                i++;
                continue;
            }

            // Normal character outside tags
            sb.Append(c);
            i++;
        }

        return sb.ToString();
    }

    // Convert a Unicode code point to string (handles surrogate pairs)
    private static string CodePointToString(int cp)
    {
        if (cp <= 0xFFFF) return ((char)cp).ToString();
        cp -= 0x10000;
        char high = (char)((cp >> 10) + 0xD800);
        char low = (char)((cp & 0x3FF) + 0xDC00);
        return new string(new[] { high, low });
    }
}

public static class StringBuilderPool
{
    private static readonly ObjectPool<StringBuilder> _pool = new(() => new StringBuilder(), sb => sb.Clear());

    public static PooledObject<StringBuilder> Get(out StringBuilder value) => _pool.Get(out value);
    public static StringBuilder Get() => _pool.Get();
    public static void Release(StringBuilder sb) => _pool.Release(sb);
}