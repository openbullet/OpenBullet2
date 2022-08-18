using RuriLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Functions.Parsing
{
    /// <summary>
    /// Provides parsing methods.
    /// </summary>
    public static class LRParser
    {
        /// <summary>
        /// Parses all strings between <paramref name="leftDelim"/> and <paramref name="rightDelim"/> in the <paramref name="input"/>.
        /// </summary>
        /// <param name="caseSensitive">Whether the case is important</param>
        public static IEnumerable<string> ParseBetween(string input, string leftDelim, string rightDelim, bool caseSensitive = true)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (leftDelim == null)
                throw new ArgumentNullException(nameof(leftDelim));

            if (rightDelim == null)
                throw new ArgumentNullException(nameof(rightDelim));

            // No delimiters = return the full input
            if (leftDelim == string.Empty && rightDelim == string.Empty)
            {
                yield return input;
                yield break;
            }

            var comp = caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

            // Left delimiter or Right delimiter not present and not empty = return nothing
            if (((leftDelim != string.Empty && !input.Contains(leftDelim, comp)) || (rightDelim != string.Empty && !input.Contains(rightDelim, comp))))
                yield break;

            while ((leftDelim == string.Empty || (input.Contains(leftDelim, comp))) && (rightDelim == string.Empty || input.Contains(rightDelim, comp)))
            {
                // Search for left delimiter and Calculate offset
                var pFrom = leftDelim == string.Empty ? 0 : input.IndexOf(leftDelim, comp) + leftDelim.Length;

                // Move right of offset
                input = input.Substring(pFrom);
                
                // Search for right delimiter and Calculate length to parse
                var pTo = rightDelim == string.Empty ? input.Length : input.IndexOf(rightDelim, comp);
                
                // Parse it
                yield return (input.Substring(0, pTo));
                
                // Move right of parsed + right
                input = input.Substring(pTo + rightDelim.Length);
            }
        }
    }
}
