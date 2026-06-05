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

        var tokenLength = 0;
        while (tokenLength < input.Length && !char.IsWhiteSpace(input[tokenLength]))
        {
            tokenLength++;
        }

        if (tokenLength == 0)
        {
            throw new Exception("Could not parse the token");
        }

        var token = input[..tokenLength];
        input = input[tokenLength..];
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

        var tokenLength = 0;
        while (tokenLength < input.Length && !char.IsWhiteSpace(input[tokenLength]))
        {
            tokenLength++;
        }

        if (tokenLength == 0)
        {
            throw new Exception("Could not parse the byte array");
        }

        var token = input[..tokenLength];

        if (!IsBase64Token(token))
        {
            throw new Exception("Could not parse the byte array");
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(token);
        }
        catch (FormatException)
        {
            throw new Exception("Could not parse the byte array");
        }

        input = input[tokenLength..];
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

        var list = new List<string>();

        // Syntax of a list of strings: ["one", "two"]
        if (!StartsWith(ref input, '['))
        {
            throw new Exception("Could not parse the list");
        }

        input = input[1..];
        input = input.TrimStart();

        // "one", "two"]
        while (!StartsWith(ref input, ']'))
        {
            EnsureHasInput(input, "Could not parse the list");

            list.Add(ParseLiteral(ref input));
            input = input.TrimStart();

            EnsureHasInput(input, "Could not parse the list");

            if (input[0] == ']')
            {
                continue;
            }

            if (input[0] != ',')
            {
                throw new Exception("Could not parse the list");
            }

            input = input[1..];
            input = input.TrimStart();
        }

        // Parse the final ]
        input = input[1..];
        input = input.TrimStart();

        return list;
    }

    /// <summary>
    /// Parses a dictionary of strings value and moves forward.
    /// </summary>
    public static Dictionary<string, string> ParseDictionary(ref string input)
    {
        input = input.TrimStart();

        var dict = new Dictionary<string, string>();

        // Syntax of a dictionary of strings: { ("key1", "value1"), ("key2", "value2") }
        if (!StartsWith(ref input, '{'))
        {
            throw new Exception("Could not parse the dictionary");
        }

        input = input[1..];
        input = input.TrimStart();

        // ("key1", "value1"), ("key2", "value2") }
        while (!StartsWith(ref input, '}'))
        {
            EnsureHasInput(input, "Could not parse the dictionary");

            if (input[0] != '(')
            {
                throw new Exception("Could not parse the dictionary");
            }

            input = input[1..];
            input = input.TrimStart();

            // "key1", "value1"), ("key2", "value2") }
            var key = ParseLiteral(ref input);

            EnsureHasInput(input, "Could not parse the dictionary");

            // , "value1"), ("key2", "value2") }
            if (input[0] != ',')
            {
                throw new Exception("Could not parse the dictionary");
            }

            input = input[1..];
            input = input.TrimStart();

            // "value1"), ("key2", "value2") }
            var value = ParseLiteral(ref input);

            dict.Add(key, value);

            EnsureHasInput(input, "Could not parse the dictionary");

            if (input[0] != ')')
            {
                throw new Exception("Could not parse the dictionary");
            }

            // Parse the )
            input = input[1..];
            input = input.TrimStart();

            EnsureHasInput(input, "Could not parse the dictionary");

            if (input[0] == '}')
            {
                continue;
            }

            if (input[0] != ',')
            {
                throw new Exception("Could not parse the dictionary");
            }

            input = input[1..];
            input = input.TrimStart();
        }

        // Parse the final }
        input = input[1..];
        input = input.TrimStart();

        return dict;
    }

    /// <summary>
    /// Parses a literal from the original <paramref name="input" /> and moves forward.
    /// </summary>
    public static string ParseLiteral(ref string input)
    {
        input = input.TrimStart();

        if (input.Length == 0 || input[0] != '"')
        {
            throw new Exception("Could not parse the literal");
        }

        var literal = new StringBuilder();

        for (var i = 1; i < input.Length; i++)
        {
            if (input[i] == '"')
            {
                input = input[(i + 1)..];
                input = input.TrimStart();
                return literal.ToString();
            }

            if (input[i] != '\\')
            {
                literal.Append(input[i]);
                continue;
            }

            i++;

            if (i >= input.Length)
            {
                throw new Exception("Could not parse the literal");
            }

            literal.Append(ParseEscapeSequence(input, ref i));
        }

        throw new Exception("Could not parse the literal");
    }

    private static void EnsureHasInput(string input, string message)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new Exception(message);
        }
    }

    private static bool StartsWith(ref string input, char c)
    {
        EnsureHasInput(input, $"Could not parse the {(c == '[' ? "list" : c == '{' ? "dictionary" : "input")}");
        return input[0] == c;
    }

    [GeneratedRegex("^(?:[Tt]rue|[Ff]alse)(?=\\s|$)")]
    private static partial Regex BoolRegex();

    [GeneratedRegex("^-?[0-9][0-9.]*(?=\\s|$)")]
    private static partial Regex FloatRegex();

    [GeneratedRegex("^-?[0-9]+(?=\\s|$)")]
    private static partial Regex IntRegex();

    private static string ParseEscapeSequence(string input, ref int index)
        => input[index] switch
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

    private static string ParseFixedLengthHexEscape(string input, ref int index, int digits)
    {
        if (index + digits >= input.Length)
        {
            throw new Exception("Could not parse the literal");
        }

        var startIndex = index + 1;
        var hex = input.Substring(startIndex, digits);

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
        var startIndex = index + 1;
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
