using System;
using System.Collections.Generic;

namespace RuriLib.Functions.Parsing;

/// <summary>
/// Provides parsing methods.
/// </summary>
public static class LRParser
{
    /// <summary>
    /// Parses all strings between <paramref name="leftDelim"/> and <paramref name="rightDelim"/> in the <paramref name="input"/>.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <param name="leftDelim">The left delimiter.</param>
    /// <param name="rightDelim">The right delimiter.</param>
    /// <param name="caseSensitive">Whether the case is important</param>
    /// <returns>The parsed values between the delimiters.</returns>
    public static IEnumerable<string> ParseBetween(string input, string leftDelim, string rightDelim, bool caseSensitive = true)
    {
        ArgumentNullException.ThrowIfNull(input);
        leftDelim ??= string.Empty;
        rightDelim ??= string.Empty;

        // No delimiters = return the full input.
        if (leftDelim == string.Empty && rightDelim == string.Empty)
        {
            yield return input;
            yield break;
        }

        var comparison = caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

        // Left delimiter or right delimiter not present and not empty = return nothing.
        if ((leftDelim != string.Empty && !input.Contains(leftDelim, comparison))
            || (rightDelim != string.Empty && !input.Contains(rightDelim, comparison)))
        {
            yield break;
        }

        while ((leftDelim == string.Empty || input.Contains(leftDelim, comparison))
            && (rightDelim == string.Empty || input.Contains(rightDelim, comparison)))
        {
            // Search for left delimiter and calculate offset.
            var fromIndex = leftDelim == string.Empty ? 0 : input.IndexOf(leftDelim, comparison) + leftDelim.Length;

            // Move right of offset.
            input = input[fromIndex..];

            // Search for right delimiter and calculate length to parse.
            var toIndex = rightDelim == string.Empty ? input.Length : input.IndexOf(rightDelim, comparison);

            // Parse it.
            yield return input[..toIndex];

            // Move right of parsed + right.
            input = input[(toIndex + rightDelim.Length)..];
        }
    }
}
