using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace RuriLib.Extensions
{
    public static class StringExtensions
    {
        public static bool AsBool(this string str)
            => bool.Parse(str);

        public static int AsInt(this string str)
            => int.Parse(str);

        public static float AsFloat(this string str)
            => float.Parse(str, NumberStyles.Any, CultureInfo.InvariantCulture);

        public static byte[] AsBytes(this string str)
            => Encoding.UTF8.GetBytes(str);

        public static string AsString(this string str)
            => str;

        public static List<string> AsList(this string str)
            => new List<string> { str };

        public static Dictionary<string, string> AsDict(this string str)
            => new Dictionary<string, string> { { str, "" } };

        /// <summary>
        /// Replaces literal values of \n, \r\n and \t with the actual escape codes.
        /// </summary>
        /// <param name="str">The string to unescape</param>
        /// <param name="useEnvNewLine">Whether to unescape both \n and \r\n with the Environment.NewLine</param>
        /// <returns>The string with unescaped escape sequences.</returns>
        public static string Unescape(this string str, bool useEnvNewLine = false)
        {
            // Unescape only \n etc. not \\n
            str = Regex.Replace(str, @"(?<!\\)\\r\\n", useEnvNewLine ? Environment.NewLine : "\r\n");
            str = Regex.Replace(str, @"(?<!\\)\\n", useEnvNewLine ? Environment.NewLine : "\n");
            str = Regex.Replace(str, @"(?<!\\)\\t", "\t");

            // Replace \\n with \n
            return new StringBuilder(str)
                .Replace(@"\\r\\n", @"\r\n")
                .Replace(@"\\n", @"\n")
                .Replace(@"\\t", @"\t")
                .ToString();
        }

        /// <summary>
        /// Pads <paramref name="str"/> by adding <paramref name="paddingCharacter"/> to its left
        /// until its length is a multiple of the <paramref name="factor"/>.
        /// </summary>
        /// <example>PadLeftToNearestMultiple("1101", 8, '0') = "00001101"</example>
        public static string PadLeftToNearestMultiple
            (this string str, int factor, char paddingCharacter = '0')
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            int totalWidth = (int)Math.Ceiling((double)str.Length / factor) * factor;
            return str.PadLeft(totalWidth, paddingCharacter);
        }

        /// <summary>
        /// Splits <paramref name="str"/> in chunks of a given size <paramref name="chunkSize"/>.
        /// </summary>
        /// <exception cref="ArgumentException">When the string's length is not a multiple of the chunk size.</exception>
        public static string[] SplitInChunks
            (this string str, int chunkSize, bool lastChunkCanBeShorter = true)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (!lastChunkCanBeShorter && str.Length % chunkSize != 0)
                throw new ArgumentException($"The input string's length must be a multiple of the chunk size ({chunkSize})");

            if (lastChunkCanBeShorter)
            {
                return Enumerable.Range(0, (int)Math.Ceiling((double)str.Length / chunkSize))
                    .Select(i => str.Substring(i * chunkSize, Math.Min(str.Length - i * chunkSize, chunkSize)))
                    .ToArray();
            }
            else
            {
                return Enumerable.Range(0, str.Length / chunkSize)
                    .Select(i => str.Substring(i * chunkSize, chunkSize))
                    .ToArray();
            }
        }

        /// <summary>
        /// Returns true if <paramref name="path"/> starts with the path <paramref name="baseDirPath"/>.
        /// Supports both relative and absolute paths.
        /// The comparison is case-insensitive on Windows, handles / and \ as folder separators and
        /// only matches if the base dir folder name is matched exactly ("c:\foobar\file.txt" is not a sub path of "c:\foo").
        /// </summary>
        public static bool IsSubPathOf(this string path, string baseDirPath)
        {
            // Fully qualify relative paths
            if (!Path.IsPathFullyQualified(path))
            {
                path = Path.Combine(baseDirPath, path);
            }

            var normalizedPath = Path.GetFullPath(path.Replace('\\', '/')
                .WithEnding("/"));

            var normalizedBaseDirPath = Path.GetFullPath(baseDirPath.Replace('\\', '/')
                .WithEnding("/"));

            // Windows filesystem is case insensitive, others are case sensitive
            var comparisonType = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            return normalizedPath.StartsWith(normalizedBaseDirPath, comparisonType);
        }

        /// <summary>
        /// Returns <paramref name="str"/> with the minimal concatenation of <paramref name="ending"/> (starting from end) that
        /// results in satisfying .EndsWith(ending).
        /// </summary>
        /// <example>"hel".WithEnding("llo") returns "hello", which is the result of "hel" + "lo".</example>
        public static string WithEnding(this string str, string ending)
        {
            if (str == null)
                return ending;

            string result = str;

            // RightMostCharacters() is 1-indexed, so include these cases
            // * Append no characters
            // * Append up to N characters, where N is ending length
            for (int i = 0; i <= ending.Length; i++)
            {
                string tmp = result + ending.RightMostCharacters(i);
                if (tmp.EndsWith(ending))
                    return tmp;
            }

            return result;
        }

        /// <summary>Gets the rightmost <paramref name="length" /> characters from a string.</summary>
        /// <param name="value">The string to retrieve the substring from. Must not be null.</param>
        /// <param name="length">The number of characters to retrieve.</param>
        /// <returns>The substring.</returns>
        public static string RightMostCharacters(this string value, int length)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length is less than zero");

            return length < value.Length
                ? value.Substring(value.Length - length)
                : value;
        }

        /// <summary>
        /// Converts a <paramref name="name"/> from e.g. "readableName" to "Readable Name"
        /// </summary>
        public static string ToReadableName(this string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (name == string.Empty)
                return name;

            var replaced = Regex.Replace(name, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1");
            return char.ToUpper(replaced[0]) + replaced.Substring(1);
        }

        /// <summary>
        /// Fixes the <paramref name="fileName"/> to be compatible with the filesystem indicization.
        /// </summary>
        /// <param name="underscore">Whether to replace the unallowed characters with an underscore instead of removing them</param>
        public static string ToValidFileName(this string fileName, bool underscore = true)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(fileName, invalidRegStr, underscore ? "_" : "").Trim();
        }

        /// <summary>
        /// Counts how many times <paramref name="text"/> occurs inside <paramref name="input"/>.
        /// </summary>
        public static int CountOccurrences(this string input, string text)
        {
            int count = 0;
            int i = 0;
            while ((i = input.IndexOf(text, i)) != -1)
            {
                i += text.Length;
                count++;
            }
            return count;
        }

        /// <summary>
        /// Truncates the <paramref name="input"/> to the <paramref name="maxLength"/> and adds [...]
        /// (only if the string is longer than <paramref name="maxLength"/>).
        /// </summary>
        public static string TruncatePretty(this string input, int maxLength)
            => input.Length <= maxLength
                ? input
                : input.Substring(0, maxLength) + " [...]";

        /// <summary>
        /// Counts the lines (amount of \r\n and \n) in a given string.
        /// </summary>
        public static int CountLines(this string input)
            => input.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Length;
    }
}
