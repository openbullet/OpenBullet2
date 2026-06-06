using System;
using System.Collections.Generic;

namespace RuriLib.Http;

internal static class CookieHeaderParser
{
    private const string ExpiresAttributePrefix = "expires=";

    internal static IEnumerable<string> SplitCookies(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        var start = 0;
        var insideExpiresAttribute = false;
        var insideQuotes = false;

        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] == '"')
            {
                insideQuotes = !insideQuotes;
                continue;
            }

            if (insideQuotes)
            {
                continue;
            }

            if (!insideExpiresAttribute && IsExpiresAttributeStart(value, i))
            {
                insideExpiresAttribute = true;
                i += ExpiresAttributePrefix.Length - 1;
                continue;
            }

            if (insideExpiresAttribute)
            {
                if (value[i] == ';')
                {
                    insideExpiresAttribute = false;
                }

                continue;
            }

            if (value[i] != ',')
            {
                continue;
            }

            var cookie = value[start..i].Trim();

            if (cookie.Length > 0)
            {
                yield return cookie;
            }

            start = i + 1;
        }

        var finalCookie = value[start..].Trim();

        if (finalCookie.Length > 0)
        {
            yield return finalCookie;
        }
    }

    private static bool IsExpiresAttributeStart(string value, int index)
    {
        if (!value.AsSpan(index).StartsWith(ExpiresAttributePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        for (var i = index - 1; i >= 0; i--)
        {
            if (value[i] == ' ')
            {
                continue;
            }

            return value[i] is ';' or ',';
        }

        return true;
    }
}
