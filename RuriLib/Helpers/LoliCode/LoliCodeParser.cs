using RuriLib.Models.Blocks;
using System;
using System.Collections.Generic;
using System.Globalization;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Variables;
using RuriLib.Models.Blocks.Settings.Interpolated;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Helpers.LoliCode;

/// <summary>
/// Has methods to parse LoliCode snippets.
/// </summary>
public static class LoliCodeParser
{
    /// <summary>
    /// Parses a setting from a LoliCode string and assigns it to the
    /// correct setting given a list of pre-initialized default settings.
    /// </summary>
    public static void ParseSetting(ref string input, Dictionary<string, BlockSetting> settings,
        BlockDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(descriptor);

        input = input.TrimStart();

        // myParam = "myValue"
        var name = LineParser.ParseToken(ref input);

        if (!descriptor.Parameters.TryGetValue(name, out var param)
            || !settings.TryGetValue(name, out var setting))
        {
            throw new Exception($"Incorrect setting name: {name}");
        }

        input = input.TrimStart();

        // = "myValue"
        if (input.Length == 0 || input[0] != '=')
        {
            throw new Exception("Could not parse the setting");
        }

        input = input[1..];
        input = input.TrimStart();

        ParseSettingValue(ref input, setting, param);
    }

    /// <summary>
    /// Parses a setting value from a LoliCode string (without the setting name) and 
    /// assigns it to the given <see cref="BlockSetting"/>.
    /// </summary>
    public static void ParseSettingValue<T>(ref string input, BlockSetting setting,
        T param) where T : BlockParameter
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(setting);
        ArgumentNullException.ThrowIfNull(param);

        // @myVariable
        // $"interp"
        // "fixedValue"
        if (input.Length > 0 && input[0] == '@') // VARIABLE
        {
            input = input[1..];

            // If there is just @ without anything after it,
            // the variable name is empty. Do not throw an exception here
            // or it will prevent saving the config (even if invalid)
            var variableName = input.Length == 0 || char.IsWhiteSpace(input[0])
                ? string.Empty
                : LineParser.ParseToken(ref input);

            setting.InputMode = SettingInputMode.Variable;
            setting.InputVariableName = variableName;
            setting.InterpolatedSetting = param switch
            {
                StringParameter x => new InterpolatedStringSetting { MultiLine = x.MultiLine },
                ListOfStringsParameter _ => new InterpolatedListOfStringsSetting(),
                DictionaryOfStringsParameter _ => new InterpolatedDictionaryOfStringsSetting(),
                _ => null
            };
            setting.FixedSetting = param switch // Initialize fixed setting as well, used for type switching
            {
                BoolParameter _ => new BoolSetting(),
                IntParameter _ => new IntSetting(),
                FloatParameter _ => new FloatSetting(),
                StringParameter x => new StringSetting { MultiLine = x.MultiLine },
                ListOfStringsParameter _ => new ListOfStringsSetting(),
                DictionaryOfStringsParameter _ => new DictionaryOfStringsSetting(),
                ByteArrayParameter _ => new ByteArraySetting(),
                EnumParameter x => new EnumSetting(x.EnumType),
                _ => throw new NotSupportedException()
            };
        }
        else if (input.Length > 0 && input[0] == '$') // INTERPOLATED
        {
            input = input[1..];
            setting.InputMode = SettingInputMode.Interpolated;
            setting.InterpolatedSetting = param switch
            {
                StringParameter x => new InterpolatedStringSetting { Value = LineParser.ParseLiteral(ref input), MultiLine = x.MultiLine },
                ListOfStringsParameter _ => new InterpolatedListOfStringsSetting { Value = LineParser.ParseList(ref input) },
                DictionaryOfStringsParameter _ => new InterpolatedDictionaryOfStringsSetting { Value = LineParser.ParseDictionary(ref input) },
                _ => throw new NotSupportedException()
            };
            setting.FixedSetting = param switch // Initialize fixed setting as well, used for type switching
            {
                StringParameter x => new StringSetting
                {
                    Value = ((InterpolatedStringSetting)setting.InterpolatedSetting).Value,
                    MultiLine = x.MultiLine
                },
                ListOfStringsParameter _ => new ListOfStringsSetting
                {
                    Value = ((InterpolatedListOfStringsSetting)setting.InterpolatedSetting).Value
                },
                DictionaryOfStringsParameter _ => new DictionaryOfStringsSetting
                {
                    Value = ((InterpolatedDictionaryOfStringsSetting)setting.InterpolatedSetting).Value
                },
                _ => throw new NotSupportedException()
            };
        }
        else // FIXED
        {
            setting.InputMode = SettingInputMode.Fixed;
            setting.FixedSetting = param switch
            {
                StringParameter x => new StringSetting { Value = LineParser.ParseLiteral(ref input), MultiLine = x.MultiLine },
                BoolParameter _ => new BoolSetting { Value = LineParser.ParseBool(ref input) },
                ByteArrayParameter _ => new ByteArraySetting { Value = LineParser.ParseByteArray(ref input) },
                DictionaryOfStringsParameter _ => new DictionaryOfStringsSetting { Value = LineParser.ParseDictionary(ref input) },
                EnumParameter x => new EnumSetting(x.EnumType) { Value = LineParser.ParseToken(ref input) },
                FloatParameter _ => new FloatSetting { Value = LineParser.ParseFloat(ref input) },
                IntParameter _ => new IntSetting { Value = LineParser.ParseInt(ref input) },
                ListOfStringsParameter _ => new ListOfStringsSetting { Value = LineParser.ParseList(ref input) },
                _ => throw new NotSupportedException()
            };
        }
    }

    /// <summary>
    /// Checks whether a line is a valid LoliCode block setting.
    /// </summary>
    public static bool IsSetting(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var i = 0;

        while (i < input.Length && char.IsWhiteSpace(input[i]))
        {
            i++;
        }

        // Must have at least one alphanumeric character (parameter name)
        if (i >= input.Length || !char.IsAsciiLetterOrDigit(input[i]))
        {
            return false;
        }

        while (i < input.Length && char.IsAsciiLetterOrDigit(input[i]))
        {
            i++;
        }

        while (i < input.Length && char.IsWhiteSpace(input[i]))
        {
            i++;
        }

        if (i >= input.Length || input[i] != '=')
        {
            return false;
        }

        i++;

        while (i < input.Length && char.IsWhiteSpace(input[i]))
        {
            i++;
        }

        return i < input.Length;
    }

    /// <summary>
    /// Detects the type of the next token in the given <paramref name="input"/>.
    /// If the token was a variable name, it returns null.
    /// </summary>
    public static VariableType? DetectTokenType(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length == 0)
        {
            throw new Exception("Could not detect the token type");
        }

        if (input.StartsWith('"'))
            return VariableType.String;

        if (input.StartsWith('['))
            return VariableType.ListOfStrings;

        if (input.StartsWith('{'))
            return VariableType.DictionaryOfStrings;

        var tokenLength = 0;
        while (tokenLength < input.Length && !char.IsWhiteSpace(input[tokenLength]))
        {
            tokenLength++;
        }

        if (tokenLength == 0)
        {
            throw new Exception("Could not detect the token type");
        }

        var token = input[..tokenLength];

        if (token.Equals("true", StringComparison.OrdinalIgnoreCase)
            || token.Equals("false", StringComparison.OrdinalIgnoreCase))
            return VariableType.Bool;

        if (IsIntToken(token))
            return VariableType.Int;

        if (IsFloatToken(token))
            return VariableType.Float;

        if (IsIdentifierToken(token))
            return null;

        if (IsBase64Token(token))
            return VariableType.ByteArray;

        throw new Exception("Could not detect the token type");
    }

    private static bool IsIntToken(string token)
    {
        var startIndex = token.StartsWith('-') ? 1 : 0;

        if (startIndex == token.Length)
        {
            return false;
        }

        for (var i = startIndex; i < token.Length; i++)
        {
            if (!char.IsAsciiDigit(token[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsFloatToken(string token)
    {
        var startIndex = token.StartsWith('-') ? 1 : 0;

        if (startIndex == token.Length)
        {
            return false;
        }

        var sawDigit = false;
        var sawDot = false;

        for (var i = startIndex; i < token.Length; i++)
        {
            if (char.IsAsciiDigit(token[i]))
            {
                sawDigit = true;
                continue;
            }

            if (token[i] == '.' && !sawDot)
            {
                sawDot = true;
                continue;
            }

            return false;
        }

        return sawDigit
            && sawDot
            && float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
    }

    private static bool IsIdentifierToken(string token)
    {
        if (token.Length == 0 || !char.IsAsciiLetter(token[0]))
        {
            return false;
        }

        for (var i = 1; i < token.Length; i++)
        {
            if (!char.IsAsciiLetterOrDigit(token[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsBase64Token(string token)
    {
        foreach (var c in token)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c is not '+' and not '/' and not '=')
            {
                return false;
            }
        }

        return token.Length > 0;
    }

    /// <summary>
    /// All the supported key identifiers.
    /// </summary>
    public static readonly string[] keyIdentifiers = ["BOOLKEY", "STRINGKEY", "INTKEY", "FLOATKEY", "LISTKEY", "DICTKEY"];

    /// <summary>
    /// Parses a <see cref="Key"/> from the input and moves forward.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="keyType"></param>
    /// <returns></returns>
    public static Key ParseKey(ref string line, string keyType) => keyType switch
    {
        "BOOLKEY" => ParseBoolKey(ref line),
        "STRINGKEY" => ParseStringKey(ref line),
        "INTKEY" => ParseIntKey(ref line),
        "FLOATKEY" => ParseFloatKey(ref line),
        "LISTKEY" => ParseListKey(ref line),
        "DICTKEY" => ParseDictKey(ref line),
        _ => throw new NotSupportedException()
    };

    private static BoolKey ParseBoolKey(ref string line)
    {
        var key = new BoolKey();
        ParseSettingValue(ref line, key.Left, new BoolParameter(string.Empty));
        key.Comparison = Enum.Parse<BoolComparison>(LineParser.ParseToken(ref line));
        ParseSettingValue(ref line, key.Right, new BoolParameter(string.Empty));
        return key;
    }

    private static StringKey ParseStringKey(ref string line)
    {
        var key = new StringKey();
        ParseSettingValue(ref line, key.Left, new StringParameter(string.Empty));
        key.Comparison = Enum.Parse<StrComparison>(LineParser.ParseToken(ref line));
        ParseSettingValue(ref line, key.Right, new StringParameter(string.Empty));
        return key;
    }

    private static IntKey ParseIntKey(ref string line)
    {
        var key = new IntKey();
        ParseSettingValue(ref line, key.Left, new IntParameter(string.Empty));
        key.Comparison = Enum.Parse<NumComparison>(LineParser.ParseToken(ref line));
        ParseSettingValue(ref line, key.Right, new IntParameter(string.Empty));
        return key;
    }

    private static FloatKey ParseFloatKey(ref string line)
    {
        var key = new FloatKey();
        ParseSettingValue(ref line, key.Left, new FloatParameter(string.Empty));
        key.Comparison = Enum.Parse<NumComparison>(LineParser.ParseToken(ref line));
        ParseSettingValue(ref line, key.Right, new FloatParameter(string.Empty));
        return key;
    }

    private static ListKey ParseListKey(ref string line)
    {
        var key = new ListKey();
        ParseSettingValue(ref line, key.Left, new ListOfStringsParameter(string.Empty));
        key.Comparison = Enum.Parse<ListComparison>(LineParser.ParseToken(ref line));
        ParseSettingValue(ref line, key.Right, new StringParameter(string.Empty));
        return key;
    }

    private static DictionaryKey ParseDictKey(ref string line)
    {
        var key = new DictionaryKey();
        ParseSettingValue(ref line, key.Left, new DictionaryOfStringsParameter(string.Empty));
        key.Comparison = Enum.Parse<DictComparison>(LineParser.ParseToken(ref line));
        ParseSettingValue(ref line, key.Right, new StringParameter(string.Empty));
        return key;
    }
}
