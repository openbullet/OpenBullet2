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

[BlockCategory("String Functions", "Blocks for working with strings", "#9acd32")]
public static class Methods
{
    #region RandomString fields

    private const string _lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string _uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string _digits = "0123456789";
    private const string _symbols = "\\!\"£$%&/()=?^'{}[]@#,;.:-_*+";
    private const string _hex = _digits + "abcdef";
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

    [Block("Removes leading or trailing whitespace from the input string")]
    public static string Trim(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();
        
        var trimmed = input.Trim();
        data.Logger.Log("Trimmed the input string", LogColors.YellowGreen);
        return trimmed;
    }

    [Block("Gets the length of a string")]
    public static int Length(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();
        
        var length = input.Length;
        data.Logger.Log($"Calculated length: {length}", LogColors.YellowGreen);
        return length;
    }

    [Block("Changes all letters of a string to uppercase")]
    public static string ToUppercase(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();
        
        var upper = input.ToUpper();
        data.Logger.Log($"Converted the input string: {upper}", LogColors.YellowGreen);
        return upper;
    }

    [Block("Changes all letters of a string to lowercase")]
    public static string ToLowercase(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();
        
        var lower = input.ToLower();
        data.Logger.Log($"Converted the input string: {lower}", LogColors.YellowGreen);
        return lower;
    }

    [Block("Replaces all occurrences of some text in a string")]
    public static string Replace(BotData data, [Variable] string original, string toReplace, string replacement)
    {
        data.Logger.LogHeader();
        
        var replaced = original.Replace(toReplace, replacement);
        data.Logger.Log($"Replaced string: {replaced}", LogColors.YellowGreen);
        return replaced;
    }

    [Block("Replaces all regex matches with a given text",
        extraInfo = "The replacement can contain regex groups with syntax like $1$2")]
    public static string RegexReplace(BotData data, [Variable] string original, string pattern, string replacement)
    {
        data.Logger.LogHeader();
        
        var replaced = Regex.Replace(original, pattern, replacement);
        data.Logger.Log($"Replaced string: {replaced}", LogColors.YellowGreen);
        return replaced;
    }

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

    [Block("URL encodes a string")]
    public static string UrlEncode(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();
        
        // The maximum allowed Uri size is 2083 characters, we use 2080 as a precaution
        var encoded = string.Join("", input.SplitInChunks(2080).Select(Uri.EscapeDataString));
        data.Logger.Log($"URL Encoded string: {encoded}", LogColors.YellowGreen);
        return encoded;
    }

    [Block("URL decodes a string")]
    public static string UrlDecode(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();
        
        var decoded = Uri.UnescapeDataString(input);
        data.Logger.Log($"URL Decoded string: {decoded}", LogColors.YellowGreen);
        return decoded;
    }

    [Block("Encodes HTML entities in a string")]
    public static string EncodeHTMLEntities(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();
        
        var encoded = WebUtility.HtmlEncode(input);
        data.Logger.Log($"Encoded string: {encoded}", LogColors.YellowGreen);
        return encoded;
    }

    [Block("Decodes HTML entities in a string")]
    public static string DecodeHTMLEntities(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();
        
        var decoded = WebUtility.HtmlDecode(input);
        data.Logger.Log($"Decoded string: {decoded}", LogColors.YellowGreen);
        return decoded;
    }

    [Block("Generates a random string given a mask",
        extraInfo = "?l = Lowercase, ?u = Uppercase, ?d = Digit, ?f = Uppercase + Lowercase, ?s = Symbol, ?h = Hex (Lowercase), ?H = Hex (Uppercase), ?m = Upper + Digits, ?n = Lower + Digits, ?i = Lower + Upper + Digits, ?a = Any, ?c = Custom")]
    public static string RandomString(BotData data, string input, string customCharset = "0123456789")
    {
        data.Logger.LogHeader();
        
        // TODO: The performance of this method can be improved by using a StringBuilder
        input = Regex.Replace(input, @"\?l", m => _lowercase[data.Random.Next(_lowercase.Length)].ToString());
        input = Regex.Replace(input, @"\?u", m => _uppercase[data.Random.Next(_uppercase.Length)].ToString());
        input = Regex.Replace(input, @"\?d", m => _digits[data.Random.Next(_digits.Length)].ToString());
        input = Regex.Replace(input, @"\?s", m => _symbols[data.Random.Next(_symbols.Length)].ToString());
        input = Regex.Replace(input, @"\?h", m => _hex[data.Random.Next(_hex.Length)].ToString());
        input = Regex.Replace(input, @"\?H", m => _hex[data.Random.Next(_hex.Length)].ToString().ToUpper());
        input = Regex.Replace(input, @"\?a", m => _allChars[data.Random.Next(_allChars.Length)].ToString());
        input = Regex.Replace(input, @"\?m", m => _udChars[data.Random.Next(_udChars.Length)].ToString());
        input = Regex.Replace(input, @"\?n", m => _ldChars[data.Random.Next(_ldChars.Length)].ToString());
        input = Regex.Replace(input, @"\?i", m => _upperLowerDigits[data.Random.Next(_upperLowerDigits.Length)].ToString());
        input = Regex.Replace(input, @"\?f", m => _upperLower[data.Random.Next(_upperLower.Length)].ToString());
        input = Regex.Replace(input, @"\?c", m => customCharset[data.Random.Next(customCharset.Length)].ToString());
        data.Logger.Log($"Generated string: {input}", LogColors.YellowGreen);
        return input;
    }

    [Block("Unescapes characters in a string")]
    public static string Unescape(BotData data, [Variable] string input)
    {
        data.Logger.LogHeader();
        
        var unescaped = Regex.Unescape(input);
        data.Logger.Log($"Unescaped: {unescaped}", LogColors.YellowGreen);
        return unescaped;
    }

    [Block("Splits a string into a list")]
    public static List<string> Split(BotData data, [Variable] string input, string separator)
    {
        data.Logger.LogHeader();
        
        var split = input.Split(separator).ToList();
        data.Logger.Log($"Split the string into {split.Count}", LogColors.YellowGreen);
        return split;
    }

    [Block("Gets the character at a specific index")]
    public static string CharAt(BotData data, [Variable] string input, int index)
    {
        data.Logger.LogHeader();
        
        var character = input[index].ToString();
        data.Logger.Log($"The character at index {index} is {character}", LogColors.YellowGreen);
        return character;
    }
}
