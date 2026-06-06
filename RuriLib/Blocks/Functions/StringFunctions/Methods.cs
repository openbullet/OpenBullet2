using RuriLib.Attributes;
using RuriLib.Extensions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace RuriLib.Blocks.Functions.String;

/// <summary>
/// Blocks for working with strings.
/// </summary>
[BlockCategory("String Functions", "Blocks for working with strings", "#9acd32")]
public static class Methods
{
    #region RandomString fields

    private const string _lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string _uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string _digits = "0123456789";
    private const string _symbols = "\\!\"£$%&/()=?^'{}[]@#,;.:-_*+";
    private const string _hex = _digits + "abcdef";
    private const string _upperHex = _digits + "ABCDEF";
    private const string _udChars = _uppercase + _digits;
    private const string _ldChars = _lowercase + _digits;
    private const string _upperLower = _lowercase + _uppercase;
    private const string _upperLowerDigits = _lowercase + _uppercase + _digits;
    private const string _allChars = _lowercase + _uppercase + _digits + _symbols;

    #endregion

    /// <summary>
    /// Rounds the value down to the nearest integer.
    /// </summary>
    [Block("Rounds the value down to the nearest integer")]
    public static int CountOccurrences(BotData data, [Variable] string input, string word)
    {
        data.Logger.LogHeader();

        var occurrences = input.CountOccurrences(word);
        data.Logger.Log($"Found {occurrences} occurrences of {word}", LogColors.YellowGreen);
        return occurrences;
    }

    /// <summary>
    /// Retrieves a piece of an input string.
    /// </summary>
    [Block("Retrieves a piece of an input string")]
    public static string Substring(BotData data, [Variable] string input, int index, int length)
    {
        data.Logger.LogHeader();

        var substring = input.Substring(index, length);
        data.Logger.Log($"Retrieved substring: {substring}", LogColors.YellowGreen);
        return substring;
    }

    /// <summary>
    /// Reverses the characters in the input string.
    /// </summary>
    [Block("Reverses the characters in the input string")]
    public static string Reverse(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();

        var charArray = input.ToCharArray();
        Array.Reverse(charArray);
        var reversed = new string(charArray);
        data.Logger.Log($"Reversed {input} with result {reversed}", LogColors.YellowGreen);
        return reversed;
    }

    /// <summary>
    /// Removes leading or trailing whitespace from the input string.
    /// </summary>
    [Block("Removes leading or trailing whitespace from the input string")]
    public static string Trim(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();

        var trimmed = input.Trim();
        data.Logger.Log("Trimmed the input string", LogColors.YellowGreen);
        return trimmed;
    }

    /// <summary>
    /// Gets the length of a string.
    /// </summary>
    [Block("Gets the length of a string")]
    public static int Length(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();

        var length = input.Length;
        data.Logger.Log($"Calculated length: {length}", LogColors.YellowGreen);
        return length;
    }

    /// <summary>
    /// Changes all letters of a string to uppercase.
    /// </summary>
    [Block("Changes all letters of a string to uppercase")]
    public static string ToUppercase(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();

        var upper = input.ToUpper();
        data.Logger.Log($"Converted the input string: {upper}", LogColors.YellowGreen);
        return upper;
    }

    /// <summary>
    /// Changes all letters of a string to lowercase.
    /// </summary>
    [Block("Changes all letters of a string to lowercase")]
    public static string ToLowercase(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();

        var lower = input.ToLower();
        data.Logger.Log($"Converted the input string: {lower}", LogColors.YellowGreen);
        return lower;
    }

    /// <summary>
    /// Replaces all occurrences of some text in a string.
    /// </summary>
    [Block("Replaces all occurrences of some text in a string")]
    public static string Replace(BotData data, [Variable] string original, string toReplace, string replacement)
    {
        data.Logger.LogHeader();

        var replaced = original.Replace(toReplace, replacement);
        data.Logger.Log($"Replaced string: {replaced}", LogColors.YellowGreen);
        return replaced;
    }

    /// <summary>
    /// Replaces all regex matches with a given text.
    /// </summary>
    [Block("Replaces all regex matches with a given text",
        extraInfo = "The replacement can contain regex groups with syntax like $1$2")]
    public static string RegexReplace(BotData data, [Variable] string original, string pattern, string replacement)
    {
        data.Logger.LogHeader();

        var replaced = Regex.Replace(original, pattern, replacement);
        data.Logger.Log($"Replaced string: {replaced}", LogColors.YellowGreen);
        return replaced;
    }

    /// <summary>
    /// Translates text in a string based on a dictionary.
    /// </summary>
    [Block("Translates text in a string basing on a dictionary")]
    public static string Translate(BotData data, [Variable] string input, Dictionary<string, string> translations,
        bool replaceOne = false)
    {
        data.Logger.LogHeader();

        var sb = new StringBuilder(input);
        var replacements = 0;

        foreach (var entry in translations.OrderBy(e => e.Key.Length).Reverse())
        {
            if (input.Contains(entry.Key))
            {
                replacements += input.CountOccurrences(entry.Key);
                sb.Replace(entry.Key, entry.Value);
                if (replaceOne) break;
            }
        }

        var translated = sb.ToString();
        data.Logger.Log($"Translated {replacements} occurrence(s). Translated string: {translated}", LogColors.YellowGreen);

        return translated;
    }

    /// <summary>
    /// URL encodes a string.
    /// </summary>
    [Block("URL encodes a string")]
    public static string UrlEncode(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();

        // The maximum allowed Uri size is 2083 characters, we use 2080 as a precaution
        var encoded = string.Join("", input.SplitInChunks(2080).Select(Uri.EscapeDataString));
        data.Logger.Log($"URL Encoded string: {encoded}", LogColors.YellowGreen);
        return encoded;
    }

    /// <summary>
    /// URL decodes a string.
    /// </summary>
    [Block("URL decodes a string")]
    public static string UrlDecode(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();

        var decoded = Uri.UnescapeDataString(input);
        data.Logger.Log($"URL Decoded string: {decoded}", LogColors.YellowGreen);
        return decoded;
    }

    /// <summary>
    /// Encodes HTML entities in a string.
    /// </summary>
    [Block("Encodes HTML entities in a string")]
    public static string EncodeHTMLEntities(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();

        var encoded = WebUtility.HtmlEncode(input);
        data.Logger.Log($"Encoded string: {encoded}", LogColors.YellowGreen);
        return encoded;
    }

    /// <summary>
    /// Decodes HTML entities in a string.
    /// </summary>
    [Block("Decodes HTML entities in a string")]
    public static string DecodeHTMLEntities(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();

        var decoded = WebUtility.HtmlDecode(input);
        data.Logger.Log($"Decoded string: {decoded}", LogColors.YellowGreen);
        return decoded;
    }

    /// <summary>
    /// Generates a random string given a mask.
    /// </summary>
    [Block("Generates a random string given a mask",
        extraInfo = "?l = Lowercase, ?u = Uppercase, ?d = Digit, ?f = Uppercase + Lowercase, ?s = Symbol, ?h = Hex (Lowercase), ?H = Hex (Uppercase), ?m = Upper + Digits, ?n = Lower + Digits, ?i = Lower + Upper + Digits, ?a = Any, ?c = Custom. Append {N} to repeat a token, e.g. ?h{10}")]
    public static string RandomString(
        BotData data,
        [BlockParam("Mask", "Mask string that mixes literal characters with tokens like ?l, ?u, ?d, ?a, or ?c. Append {N} after a token to repeat it, for example ?h{10}.")]
        string input,
        [BlockParam("Custom Charset", "Character set used only when the mask contains the ?c token.")]
        string customCharset = "0123456789")
    {
        data.Logger.LogHeader();

        var output = new StringBuilder(input.Length);

        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] == '?' &&
                i + 1 < input.Length &&
                TryGetRandomCharset(input[i + 1], customCharset, out var charset))
            {
                var repeatCount = 1;
                var lastConsumedIndex = i + 1;

                if (TryParseRepeatCount(input, i + 2, out var parsedRepeatCount, out var closingBraceIndex))
                {
                    repeatCount = parsedRepeatCount;
                    lastConsumedIndex = closingBraceIndex;
                }

                AppendRandomChars(output, data.Random, charset, repeatCount);
                i = lastConsumedIndex;
                continue;
            }

            output.Append(input[i]);
        }

        var generated = output.ToString();
        data.Logger.Log($"Generated string: {generated}", LogColors.YellowGreen);
        return generated;
    }

    /// <summary>
    /// Generates a GUID using the selected version and output format.
    /// </summary>
    [Block("Generates a GUID using the selected version and output format", name = "Generate GUID")]
    public static string GenerateGuid(
        BotData data,
        [BlockParam("Version", "Choose whether to generate a random GUID (v4) or a time-ordered GUID (v7).")]
        GuidVersion version = GuidVersion.V4,
        [BlockParam("Format", "Choose the string format: D = hyphenated, N = compact, B = braces, P = parentheses.")]
        GuidFormat format = GuidFormat.D)
    {
        data.Logger.LogHeader();

        var guid = version switch
        {
            GuidVersion.V4 => Guid.NewGuid(),
            GuidVersion.V7 => Guid.CreateVersion7(),
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };

        var formatted = guid.ToString(format.ToString());
        data.Logger.Log($"Generated {version} GUID: {formatted}", LogColors.YellowGreen);
        return formatted;
    }

    private static bool TryGetRandomCharset(char token, string customCharset, out string charset)
    {
        charset = token switch
        {
            'l' => _lowercase,
            'u' => _uppercase,
            'd' => _digits,
            's' => _symbols,
            'h' => _hex,
            'H' => _upperHex,
            'a' => _allChars,
            'm' => _udChars,
            'n' => _ldChars,
            'i' => _upperLowerDigits,
            'f' => _upperLower,
            'c' => customCharset,
            _ => string.Empty
        };

        return token is 'l' or 'u' or 'd' or 's' or 'h' or 'H' or 'a' or 'm' or 'n' or 'i' or 'f' or 'c';
    }

    private static bool TryParseRepeatCount(string input, int startIndex, out int repeatCount, out int closingBraceIndex)
    {
        repeatCount = 1;
        closingBraceIndex = startIndex - 1;

        if (startIndex >= input.Length || input[startIndex] != '{')
        {
            return false;
        }

        var closingBrace = input.IndexOf('}', startIndex + 1);

        if (closingBrace <= startIndex + 1)
        {
            return false;
        }

        if (!int.TryParse(input.Substring(startIndex + 1, closingBrace - startIndex - 1), out repeatCount) ||
            repeatCount < 0)
        {
            return false;
        }

        closingBraceIndex = closingBrace;
        return true;
    }

    private static void AppendRandomChars(StringBuilder output, Random random, string charset, int repeatCount)
    {
        for (var i = 0; i < repeatCount; i++)
        {
            output.Append(charset[random.Next(charset.Length)]);
        }
    }

    /// <summary>
    /// Unescapes characters in a string.
    /// </summary>
    [Block("Unescapes characters in a string")]
    public static string Unescape(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();

        var unescaped = Regex.Unescape(input);
        data.Logger.Log($"Unescaped: {unescaped}", LogColors.YellowGreen);
        return unescaped;
    }

    /// <summary>
    /// Splits a string into a list.
    /// </summary>
    [Block("Splits a string into a list")]
    public static List<string> Split(BotData data, [Variable] string input, string separator)
    {
        data.Logger.LogHeader();

        var split = input.Split(separator).ToList();
        data.Logger.Log($"Split the string into {split.Count}", LogColors.YellowGreen);
        return split;
    }

    /// <summary>
    /// Gets the character at a specific index.
    /// </summary>
    [Block("Gets the character at a specific index")]
    public static string CharAt(BotData data, [Variable] string input, int index)
    {
        data.Logger.LogHeader();

        var character = input[index].ToString();
        data.Logger.Log($"The character at index {index} is {character}", LogColors.YellowGreen);
        return character;
    }
}

/// <summary>
/// Supported GUID generation strategies.
/// </summary>
public enum GuidVersion
{
    /// <summary>
    /// Random GUID.
    /// </summary>
    V4,

    /// <summary>
    /// Time-ordered GUID.
    /// </summary>
    V7
}

/// <summary>
/// Supported GUID string formats.
/// </summary>
public enum GuidFormat
{
    /// <summary>
    /// Hyphenated 32-digit format.
    /// </summary>
    D,

    /// <summary>
    /// Compact 32-digit format.
    /// </summary>
    N,

    /// <summary>
    /// Hyphenated format wrapped in braces.
    /// </summary>
    B,

    /// <summary>
    /// Hyphenated format wrapped in parentheses.
    /// </summary>
    P
}
