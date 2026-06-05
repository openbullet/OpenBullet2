using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RuriLib.Helpers.LoliCode;

/// <summary>
/// Has methods to parse LoliCode tokens.
/// </summary>
public static partial class LineParser
{
    /// <summary>
    /// Parses a generic LoliCode token (anything until a whitespace character) and moves forward.
    /// </summary>
    public static string ParseToken(ref string input)
    {
        input = input.TrimStart();
        var index = 0;
        var token = ParseToken(input, ref index);
        input = input[index..];
        input = input.TrimStart();
        return token;
    }

    /// <summary>
    /// Parses an <see cref="int" /> value and moves forward.
    /// </summary>
    public static int ParseInt(ref string input)
    {
        input = input.TrimStart();

        var match = IntRegex().Match(input);

        if (!match.Success)
        {
            throw new Exception("Could not parse the int");
        }

        input = input[match.Value.Length..];
        input = input.TrimStart();

        return int.Parse(match.Value);
    }

    /// <summary>
    /// Parses a <see cref="float" /> value and moves forward.
    /// </summary>
    public static float ParseFloat(ref string input)
    {
        input = input.TrimStart();

        var match = FloatRegex().Match(input);

        if (!match.Success)
        {
            throw new Exception("Could not parse the float");
        }

        input = input[match.Value.Length..];
        input = input.TrimStart();

        return float.Parse(match.Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parses an array of <see cref="byte" /> value and moves forward.
    /// </summary>
    public static byte[] ParseByteArray(ref string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        input = input.TrimStart();
        var index = 0;
        var bytes = ParseByteArray(input, ref index);
        input = input[index..];
        input = input.TrimStart();
        return bytes;
    }

    /// <summary>
    /// Parses a <see cref="bool" /> value and moves forward.
    /// </summary>
    public static bool ParseBool(ref string input)
    {
        input = input.TrimStart();

        var match = BoolRegex().Match(input);

        if (!match.Success)
        {
            throw new Exception("Could not parse the bool");
        }

        input = input[match.Value.Length..];
        input = input.TrimStart();

        return bool.Parse(match.Value);
    }

    /// <summary>
    /// Parses a list of strings value and moves forward.
    /// </summary>
    public static List<string> ParseList(ref string input)
    {
        input = input.TrimStart();
        var index = 0;
        var list = ParseList(input, ref index);
        input = input[index..];
        input = input.TrimStart();
        return list;
    }

    /// <summary>
    /// Parses a dictionary of strings value and moves forward.
    /// </summary>
    public static Dictionary<string, string> ParseDictionary(ref string input)
    {
        input = input.TrimStart();
        var index = 0;
        var dict = ParseDictionary(input, ref index);
        input = input[index..];
        input = input.TrimStart();
        return dict;
    }

    /// <summary>
    /// Parses a literal from the original <paramref name="input" /> and moves forward.
    /// </summary>
    public static string ParseLiteral(ref string input)
    {
        input = input.TrimStart();
        var index = 0;
        var literal = ParseLiteral(input, ref index);
        input = input[index..];
        input = input.TrimStart();
        return literal;
    }

    private static string ParseToken(string input, ref int index)
    {
        var startIndex = index;
        while (index < input.Length && !char.IsWhiteSpace(input[index]))
        {
            index++;
        }

        if (index == startIndex)
        {
            throw new Exception("Could not parse the token");
        }

        return input[startIndex..index];
    }

    private static byte[] ParseByteArray(string input, ref int index)
    {
        var token = ParseToken(input, ref index);

        if (!IsBase64Token(token))
        {
            throw new Exception("Could not parse the byte array");
        }

        try
        {
            return Convert.FromBase64String(token);
        }
        catch (FormatException)
        {
            throw new Exception("Could not parse the byte array");
        }
    }

    private static List<string> ParseList(string input, ref int index)
    {
        const string errorMessage = "Could not parse the list";

        ExpectChar(input, ref index, '[', errorMessage);
        SkipWhitespace(input, ref index);

        var list = new List<string>();
        if (TryConsumeChar(input, ref index, ']'))
        {
            return list;
        }

        while (true)
        {
            list.Add(ParseLiteral(input, ref index));
            SkipWhitespace(input, ref index);

            if (TryConsumeChar(input, ref index, ']'))
            {
                return list;
            }

            ExpectChar(input, ref index, ',', errorMessage);
            SkipWhitespace(input, ref index);
        }
    }

    private static Dictionary<string, string> ParseDictionary(string input, ref int index)
    {
        const string errorMessage = "Could not parse the dictionary";

        ExpectChar(input, ref index, '{', errorMessage);
        SkipWhitespace(input, ref index);

        var dict = new Dictionary<string, string>();
        if (TryConsumeChar(input, ref index, '}'))
        {
            return dict;
        }

        while (true)
        {
            ExpectChar(input, ref index, '(', errorMessage);
            SkipWhitespace(input, ref index);

            var key = ParseLiteral(input, ref index);
            SkipWhitespace(input, ref index);

            ExpectChar(input, ref index, ',', errorMessage);
            SkipWhitespace(input, ref index);

            var value = ParseLiteral(input, ref index);
            SkipWhitespace(input, ref index);

            dict.Add(key, value);

            ExpectChar(input, ref index, ')', errorMessage);
            SkipWhitespace(input, ref index);

            if (TryConsumeChar(input, ref index, '}'))
            {
                return dict;
            }

            ExpectChar(input, ref index, ',', errorMessage);
            SkipWhitespace(input, ref index);
        }
    }

    private static string ParseLiteral(string input, ref int index)
    {
        const string errorMessage = "Could not parse the literal";

        ExpectChar(input, ref index, '"', errorMessage);

        var literal = new StringBuilder();
        while (index < input.Length)
        {
            var c = input[index++];

            if (c == '"')
            {
                return literal.ToString();
            }

            if (c != '\\')
            {
                literal.Append(c);
                continue;
            }

            literal.Append(ParseEscapeSequence(input, ref index));
        }

        throw new Exception(errorMessage);
    }

    [GeneratedRegex("^(?:[Tt]rue|[Ff]alse)(?=\\s|$)")]
    private static partial Regex BoolRegex();

    [GeneratedRegex("^-?[0-9][0-9.]*(?=\\s|$)")]
    private static partial Regex FloatRegex();

    [GeneratedRegex("^-?[0-9]+(?=\\s|$)")]
    private static partial Regex IntRegex();

    private static string ParseEscapeSequence(string input, ref int index)
    {
        if (index >= input.Length)
        {
            throw new Exception("Could not parse the literal");
        }

        var escapedChar = input[index++];

        return escapedChar switch
        {
            '"' => "\"",
            '\\' => "\\",
            '/' => "/",
            '\'' => "'",
            '0' => "\0",
            'a' => "\a",
            'b' => "\b",
            'f' => "\f",
            'n' => "\n",
            'r' => "\r",
            't' => "\t",
            'v' => "\v",
            'u' => ParseFixedLengthHexEscape(input, ref index, 4),
            'U' => ParseFixedLengthHexEscape(input, ref index, 8),
            'x' => ParseVariableLengthHexEscape(input, ref index),
            _ => throw new Exception("Could not parse the literal")
        };
    }

    private static string ParseFixedLengthHexEscape(string input, ref int index, int digits)
    {
        if (index + digits > input.Length)
        {
            throw new Exception("Could not parse the literal");
        }

        var hex = input.Substring(index, digits);

        if (!uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
        {
            throw new Exception("Could not parse the literal");
        }

        index += digits;

        if (digits == 8)
        {
            return char.ConvertFromUtf32(checked((int)code));
        }

        return ((char)code).ToString();
    }

    private static string ParseVariableLengthHexEscape(string input, ref int index)
    {
        var startIndex = index;
        var length = 0;

        while (startIndex + length < input.Length
            && length < 4
            && Uri.IsHexDigit(input[startIndex + length]))
        {
            length++;
        }

        if (length == 0)
        {
            throw new Exception("Could not parse the literal");
        }

        var hex = input.Substring(startIndex, length);

        if (!ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
        {
            throw new Exception("Could not parse the literal");
        }

        index += length;
        return ((char)code).ToString();
    }

    private static void SkipWhitespace(string input, ref int index)
    {
        while (index < input.Length && char.IsWhiteSpace(input[index]))
        {
            index++;
        }
    }

    private static void ExpectChar(string input, ref int index, char c, string errorMessage)
    {
        if (!TryConsumeChar(input, ref index, c))
        {
            throw new Exception(errorMessage);
        }
    }

    private static bool TryConsumeChar(string input, ref int index, char c)
    {
        if (index >= input.Length || input[index] != c)
        {
            return false;
        }

        index++;
        return true;
    }

    private static bool IsBase64Token(string token)
    {
        if (token.Length == 0 || token.Length % 4 != 0)
        {
            return false;
        }

        var paddingStart = token.IndexOf('=');
        var contentLength = paddingStart >= 0 ? paddingStart : token.Length;

        for (var i = 0; i < contentLength; i++)
        {
            if (!char.IsAsciiLetterOrDigit(token[i]) && token[i] is not '+' and not '/')
            {
                return false;
            }
        }

        if (paddingStart < 0)
        {
            return true;
        }

        var paddingLength = token.Length - paddingStart;
        if (paddingLength > 2)
        {
            return false;
        }

        for (var i = paddingStart; i < token.Length; i++)
        {
            if (token[i] != '=')
            {
                return false;
            }
        }

        return true;
    }
}
