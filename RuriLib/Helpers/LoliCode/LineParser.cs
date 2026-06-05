using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RuriLib.Helpers.LoliCode;

/// <summary>
/// Has methods to parse LoliCode tokens.
/// </summary>
public static class LineParser
{
    /// <summary>
    /// Parses a generic LoliCode token (anything until a whitespace character) and moves forward.
    /// </summary>
    public static string ParseToken(ref string input)
    {
        input = input.TrimStart();
        var index = 0;
        var token = ParseToken(input.AsSpan(), ref index);
        AdvanceInput(ref input, index);
        return token;
    }

    /// <summary>
    /// Parses an <see cref="int" /> value and moves forward.
    /// </summary>
    public static int ParseInt(ref string input)
    {
        input = input.TrimStart();
        var index = 0;
        var value = ParseInt(input.AsSpan(), ref index);
        AdvanceInput(ref input, index);
        return value;
    }

    /// <summary>
    /// Parses a <see cref="float" /> value and moves forward.
    /// </summary>
    public static float ParseFloat(ref string input)
    {
        input = input.TrimStart();
        var index = 0;
        var value = ParseFloat(input.AsSpan(), ref index);
        AdvanceInput(ref input, index);
        return value;
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
        var bytes = ParseByteArray(input.AsSpan(), ref index);
        AdvanceInput(ref input, index);
        return bytes;
    }

    /// <summary>
    /// Parses a <see cref="bool" /> value and moves forward.
    /// </summary>
    public static bool ParseBool(ref string input)
    {
        input = input.TrimStart();
        var index = 0;
        var value = ParseBool(input.AsSpan(), ref index);
        AdvanceInput(ref input, index);
        return value;
    }

    /// <summary>
    /// Parses a list of strings value and moves forward.
    /// </summary>
    public static List<string> ParseList(ref string input)
    {
        input = input.TrimStart();
        var index = 0;
        var list = ParseList(input.AsSpan(), ref index);
        AdvanceInput(ref input, index);
        return list;
    }

    /// <summary>
    /// Parses a dictionary of strings value and moves forward.
    /// </summary>
    public static Dictionary<string, string> ParseDictionary(ref string input)
    {
        input = input.TrimStart();
        var index = 0;
        var dict = ParseDictionary(input.AsSpan(), ref index);
        AdvanceInput(ref input, index);
        return dict;
    }

    /// <summary>
    /// Parses a literal from the original <paramref name="input" /> and moves forward.
    /// </summary>
    public static string ParseLiteral(ref string input)
    {
        input = input.TrimStart();
        var index = 0;
        var literal = ParseLiteral(input.AsSpan(), ref index);
        AdvanceInput(ref input, index);
        return literal;
    }

    private static string ParseToken(ReadOnlySpan<char> input, ref int index)
    {
        var token = ParseTokenSpan(input, ref index, "Could not parse the token");
        return token.ToString();
    }

    private static int ParseInt(ReadOnlySpan<char> input, ref int index)
    {
        var token = ParseTokenSpan(input, ref index, "Could not parse the int");

        if (!IsIntToken(token)
            || !int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw new Exception("Could not parse the int");
        }

        return value;
    }

    private static float ParseFloat(ReadOnlySpan<char> input, ref int index)
    {
        var token = ParseTokenSpan(input, ref index, "Could not parse the float");

        if (!IsFloatToken(token)
            || !float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw new Exception("Could not parse the float");
        }

        return value;
    }

    private static byte[] ParseByteArray(ReadOnlySpan<char> input, ref int index)
    {
        var token = ParseTokenSpan(input, ref index, "Could not parse the byte array");

        if (!IsBase64Token(token))
        {
            throw new Exception("Could not parse the byte array");
        }

        var decodedLength = GetDecodedBase64Length(token);
        var bytes = new byte[decodedLength];

        if (!Convert.TryFromBase64Chars(token, bytes, out var bytesWritten) || bytesWritten != decodedLength)
        {
            throw new Exception("Could not parse the byte array");
        }

        return bytes;
    }

    private static bool ParseBool(ReadOnlySpan<char> input, ref int index)
    {
        var token = ParseTokenSpan(input, ref index, "Could not parse the bool");

        if (!bool.TryParse(token, out var value))
        {
            throw new Exception("Could not parse the bool");
        }

        return value;
    }

    private static List<string> ParseList(ReadOnlySpan<char> input, ref int index)
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

    private static Dictionary<string, string> ParseDictionary(ReadOnlySpan<char> input, ref int index)
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

    private static string ParseLiteral(ReadOnlySpan<char> input, ref int index)
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

    private static ReadOnlySpan<char> ParseTokenSpan(ReadOnlySpan<char> input, ref int index, string errorMessage)
    {
        var startIndex = index;
        while (index < input.Length && !char.IsWhiteSpace(input[index]))
        {
            index++;
        }

        if (index == startIndex)
        {
            throw new Exception(errorMessage);
        }

        return input[startIndex..index];
    }

    private static string ParseEscapeSequence(ReadOnlySpan<char> input, ref int index)
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

    private static string ParseFixedLengthHexEscape(ReadOnlySpan<char> input, ref int index, int digits)
    {
        if (index + digits > input.Length)
        {
            throw new Exception("Could not parse the literal");
        }

        var hex = input[index..(index + digits)];

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

    private static string ParseVariableLengthHexEscape(ReadOnlySpan<char> input, ref int index)
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

        var hex = input.Slice(startIndex, length);

        if (!ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
        {
            throw new Exception("Could not parse the literal");
        }

        index += length;
        return ((char)code).ToString();
    }

    private static void AdvanceInput(ref string input, int index)
    {
        input = input[index..];
        input = input.TrimStart();
    }

    private static void SkipWhitespace(ReadOnlySpan<char> input, ref int index)
    {
        while (index < input.Length && char.IsWhiteSpace(input[index]))
        {
            index++;
        }
    }

    private static void ExpectChar(ReadOnlySpan<char> input, ref int index, char c, string errorMessage)
    {
        if (!TryConsumeChar(input, ref index, c))
        {
            throw new Exception(errorMessage);
        }
    }

    private static bool TryConsumeChar(ReadOnlySpan<char> input, ref int index, char c)
    {
        if (index >= input.Length || input[index] != c)
        {
            return false;
        }

        index++;
        return true;
    }

    private static bool IsIntToken(ReadOnlySpan<char> token)
    {
        if (token.Length == 0)
        {
            return false;
        }

        var index = token[0] == '-' ? 1 : 0;
        if (index == token.Length)
        {
            return false;
        }

        for (; index < token.Length; index++)
        {
            if (!char.IsAsciiDigit(token[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsFloatToken(ReadOnlySpan<char> token)
    {
        if (token.Length == 0)
        {
            return false;
        }

        var index = token[0] == '-' ? 1 : 0;
        if (index == token.Length || !char.IsAsciiDigit(token[index]))
        {
            return false;
        }

        var sawDot = false;
        for (; index < token.Length; index++)
        {
            if (char.IsAsciiDigit(token[index]))
            {
                continue;
            }

            if (token[index] == '.' && !sawDot)
            {
                sawDot = true;
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool IsBase64Token(ReadOnlySpan<char> token)
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

    private static int GetDecodedBase64Length(ReadOnlySpan<char> token)
    {
        var padding = 0;
        if (token.Length > 0 && token[^1] == '=')
        {
            padding++;
        }

        if (token.Length > 1 && token[^2] == '=')
        {
            padding++;
        }

        return (token.Length / 4) * 3 - padding;
    }
}
