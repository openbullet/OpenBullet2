using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RuriLib.Exceptions;

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
        var cursor = new LineParserCursor(input);
        cursor.SkipWhitespace();
        var token = ParseToken(cursor);
        AdvanceInput(ref input, cursor.Index);
        return token;
    }

    /// <summary>
    /// Parses an <see cref="int" /> value and moves forward.
    /// </summary>
    public static int ParseInt(ref string input)
    {
        var cursor = new LineParserCursor(input);
        cursor.SkipWhitespace();
        var value = ParseInt(cursor);
        AdvanceInput(ref input, cursor.Index);
        return value;
    }

    /// <summary>
    /// Parses a <see cref="float" /> value and moves forward.
    /// </summary>
    public static float ParseFloat(ref string input)
    {
        var cursor = new LineParserCursor(input);
        cursor.SkipWhitespace();
        var value = ParseFloat(cursor);
        AdvanceInput(ref input, cursor.Index);
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

        var cursor = new LineParserCursor(input);
        cursor.SkipWhitespace();
        var bytes = ParseByteArray(cursor);
        AdvanceInput(ref input, cursor.Index);
        return bytes;
    }

    /// <summary>
    /// Parses a <see cref="bool" /> value and moves forward.
    /// </summary>
    public static bool ParseBool(ref string input)
    {
        var cursor = new LineParserCursor(input);
        cursor.SkipWhitespace();
        var value = ParseBool(cursor);
        AdvanceInput(ref input, cursor.Index);
        return value;
    }

    /// <summary>
    /// Parses a list of strings value and moves forward.
    /// </summary>
    public static List<string> ParseList(ref string input)
    {
        var cursor = new LineParserCursor(input);
        cursor.SkipWhitespace();
        var list = ParseList(cursor);
        AdvanceInput(ref input, cursor.Index);
        return list;
    }

    /// <summary>
    /// Parses a dictionary of strings value and moves forward.
    /// </summary>
    public static Dictionary<string, string> ParseDictionary(ref string input)
    {
        var cursor = new LineParserCursor(input);
        cursor.SkipWhitespace();
        var dict = ParseDictionary(cursor);
        AdvanceInput(ref input, cursor.Index);
        return dict;
    }

    /// <summary>
    /// Parses a literal from the original <paramref name="input" /> and moves forward.
    /// </summary>
    public static string ParseLiteral(ref string input)
    {
        var cursor = new LineParserCursor(input);
        cursor.SkipWhitespace();
        var literal = ParseLiteral(cursor);
        AdvanceInput(ref input, cursor.Index);
        return literal;
    }

    private static string ParseToken(LineParserCursor cursor)
    {
        var token = ParseTokenSpan(cursor, "Expected token");
        return token.ToString();
    }

    private static int ParseInt(LineParserCursor cursor)
    {
        var token = ParseTokenSpan(cursor, "Expected int");

        if (!IsIntToken(token)
            || !int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            throw cursor.CreateExceptionAt(cursor.LastTokenStartIndex, $"Invalid integer token '{token}'");
        }

        return value;
    }

    private static float ParseFloat(LineParserCursor cursor)
    {
        var token = ParseTokenSpan(cursor, "Expected float");

        if (!IsFloatToken(token)
            || !float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw cursor.CreateExceptionAt(cursor.LastTokenStartIndex, $"Invalid float token '{token}'");
        }

        return value;
    }

    private static byte[] ParseByteArray(LineParserCursor cursor)
    {
        var token = ParseTokenSpan(cursor, "Expected byte array");

        if (!IsBase64Token(token))
        {
            throw cursor.CreateExceptionAt(cursor.LastTokenStartIndex, "Invalid base64 byte array");
        }

        var decodedLength = GetDecodedBase64Length(token);
        var bytes = new byte[decodedLength];

        if (!Convert.TryFromBase64Chars(token, bytes, out var bytesWritten) || bytesWritten != decodedLength)
        {
            throw cursor.CreateExceptionAt(cursor.LastTokenStartIndex, "Invalid base64 byte array");
        }

        return bytes;
    }

    private static bool ParseBool(LineParserCursor cursor)
    {
        var token = ParseTokenSpan(cursor, "Expected bool");

        if (!bool.TryParse(token, out var value))
        {
            throw cursor.CreateExceptionAt(cursor.LastTokenStartIndex, $"Invalid bool token '{token}'");
        }

        return value;
    }

    private static List<string> ParseList(LineParserCursor cursor)
    {
        ExpectChar(cursor, '[', "to start a list");
        cursor.SkipWhitespace();

        var list = new List<string>();
        if (cursor.TryConsume(']'))
        {
            return list;
        }

        while (true)
        {
            list.Add(ParseLiteral(cursor));
            cursor.SkipWhitespace();

            if (cursor.TryConsume(']'))
            {
                return list;
            }

            ExpectChar(cursor, ',', "between list items");
            cursor.SkipWhitespace();
        }
    }

    private static Dictionary<string, string> ParseDictionary(LineParserCursor cursor)
    {
        ExpectChar(cursor, '{', "to start a dictionary");
        cursor.SkipWhitespace();

        var dict = new Dictionary<string, string>();
        if (cursor.TryConsume('}'))
        {
            return dict;
        }

        while (true)
        {
            ExpectChar(cursor, '(', "to start a dictionary entry");
            cursor.SkipWhitespace();

            var key = ParseLiteral(cursor);
            cursor.SkipWhitespace();

            ExpectChar(cursor, ',', "between dictionary key and value");
            cursor.SkipWhitespace();

            var value = ParseLiteral(cursor);
            cursor.SkipWhitespace();

            dict.Add(key, value);

            ExpectChar(cursor, ')', "to close a dictionary entry");
            cursor.SkipWhitespace();

            if (cursor.TryConsume('}'))
            {
                return dict;
            }

            ExpectChar(cursor, ',', "between dictionary entries");
            cursor.SkipWhitespace();
        }
    }

    private static string ParseLiteral(LineParserCursor cursor)
    {
        ExpectChar(cursor, '"', "to start a string literal");

        var literal = new StringBuilder();
        while (!cursor.IsAtEnd)
        {
            var c = cursor.Read();

            if (c == '"')
            {
                return literal.ToString();
            }

            if (c != '\\')
            {
                literal.Append(c);
                continue;
            }

            literal.Append(ParseEscapeSequence(cursor));
        }

        throw cursor.CreateException("Unterminated string literal");
    }

    private static ReadOnlySpan<char> ParseTokenSpan(LineParserCursor cursor, string errorMessage)
    {
        var startIndex = cursor.Index;
        cursor.LastTokenStartIndex = startIndex;

        while (!cursor.IsAtEnd && !char.IsWhiteSpace(cursor.Current))
        {
            cursor.Advance();
        }

        if (cursor.Index == startIndex)
        {
            throw cursor.CreateException(errorMessage);
        }

        return cursor.Input.AsSpan(startIndex, cursor.Index - startIndex);
    }

    private static string ParseEscapeSequence(LineParserCursor cursor)
    {
        if (cursor.IsAtEnd)
        {
            throw cursor.CreateException("Unterminated escape sequence");
        }

        var escapeIndex = cursor.Index;
        var escapedChar = cursor.Read();

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
            'u' => ParseFixedLengthHexEscape(cursor, 4),
            'U' => ParseFixedLengthHexEscape(cursor, 8),
            'x' => ParseVariableLengthHexEscape(cursor),
            _ => throw cursor.CreateExceptionAt(escapeIndex, $"Invalid escape sequence '\\{escapedChar}'")
        };
    }

    private static string ParseFixedLengthHexEscape(LineParserCursor cursor, int digits)
    {
        if (cursor.Index + digits > cursor.Input.Length)
        {
            throw cursor.CreateException($"Expected {digits} hex digits in escape sequence");
        }

        var hex = cursor.Input.AsSpan(cursor.Index, digits);

        if (!uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
        {
            throw cursor.CreateException($"Invalid hex escape sequence '{hex}'");
        }

        cursor.Advance(digits);

        if (digits == 8)
        {
            return char.ConvertFromUtf32(checked((int)code));
        }

        return ((char)code).ToString();
    }

    private static string ParseVariableLengthHexEscape(LineParserCursor cursor)
    {
        var startIndex = cursor.Index;
        var length = 0;

        while (startIndex + length < cursor.Input.Length
            && length < 4
            && Uri.IsHexDigit(cursor.Input[startIndex + length]))
        {
            length++;
        }

        if (length == 0)
        {
            throw cursor.CreateException("Expected at least one hex digit in escape sequence");
        }

        var hex = cursor.Input.AsSpan(startIndex, length);

        if (!ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
        {
            throw cursor.CreateException($"Invalid hex escape sequence '{hex}'");
        }

        cursor.Advance(length);
        return ((char)code).ToString();
    }

    private static void AdvanceInput(ref string input, int index)
    {
        input = input[index..];
        input = input.TrimStart();
    }

    private static void ExpectChar(LineParserCursor cursor, char c, string expectation)
    {
        if (!cursor.TryConsume(c))
        {
            throw cursor.CreateException($"Expected '{c}' {expectation}, found {cursor.DescribeCurrent()}");
        }
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

    private sealed class LineParserCursor
    {
        public LineParserCursor(string input)
            => Input = input;

        public string Input { get; }

        public int Index { get; private set; }

        public int LastTokenStartIndex { get; set; }

        public bool IsAtEnd => Index >= Input.Length;

        public char Current => Input[Index];

        public int ColumnNumber
            => GetColumnNumber(Index);

        public void Advance(int count = 1) => Index += count;

        public char Read() => Input[Index++];

        public void SkipWhitespace()
        {
            while (!IsAtEnd && char.IsWhiteSpace(Current))
            {
                Advance();
            }
        }

        public bool TryConsume(char c)
        {
            if (IsAtEnd || Current != c)
            {
                return false;
            }

            Advance();
            return true;
        }

        public LineParsingException CreateException(string message)
            => new(ColumnNumber, message);

        public LineParsingException CreateExceptionAt(int index, string message)
            => new(GetColumnNumber(index), message);

        public string DescribeCurrent()
            => IsAtEnd ? "end of line" : $"'{Current}'";

        private int GetColumnNumber(int index)
        {
            var lineStartIndex = index;

            while (lineStartIndex > 0 && Input[lineStartIndex - 1] is not '\r' and not '\n')
            {
                lineStartIndex--;
            }

            return index - lineStartIndex + 1;
        }
    }
}
