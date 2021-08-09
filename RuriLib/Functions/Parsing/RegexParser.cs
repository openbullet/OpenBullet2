using RuriLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RuriLib.Functions.Parsing
{
    public static class RegexParser
    {
        /// <summary>
        /// Parses a string via a Regex pattern containing Groups, then returns them according to an output format.
        /// </summary>
        /// <param name="input">The string to parse</param>
        /// <param name="pattern">The Regex pattern containing groups</param>
        /// <param name="outputFormat">The output format string, for which [0] will be replaced with the full match,
        /// [1] with the first group etc.</param>
        /// <param name="options">The Regex Options to use</param>
        public static IEnumerable<string> MatchGroupsToString
            (string input, string pattern, string outputFormat, RegexOptions options = RegexOptions.None)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            if (outputFormat == null)
                throw new ArgumentNullException(nameof(outputFormat));

            // Replacing \r\n with \n if multiline enabled
            input = options.HasFlag(RegexOptions.Multiline) ? input.Replace("\r\n", "\n") : input;

            return Regex.Matches(input, pattern, options)
                .Where(m => m.Success)
                .Select(m => m.Groups.ToString(outputFormat));
        }

        private static string ToString(this GroupCollection groups, string outputFormat)
        {
            StringBuilder sb = new StringBuilder(outputFormat);

            for (int i = 0; i < groups.Count; i++)
                sb.Replace($"[{i}]", groups[i].Value);

            return sb.ToString();
        }
    }
}
