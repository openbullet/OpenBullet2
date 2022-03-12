using RuriLib.Models.Blocks;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Variables;
using RuriLib.Models.Blocks.Settings.Interpolated;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Helpers.LoliCode
{
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
            input = input.TrimStart();

            // myParam = "myValue"
            var name = LineParser.ParseToken(ref input);

            if (!descriptor.Parameters.ContainsKey(name) || !settings.ContainsKey(name))
                throw new Exception($"Incorrect setting name: {name}");

            var param = descriptor.Parameters[name];
            var setting = settings[name];

            input = input.TrimStart();

            // = "myValue"
            if (input[0] != '=')
                throw new Exception("Could not parse the setting");

            input = input.Substring(1);
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
            // @myVariable
            // $"interp"
            // "fixedValue"
            if (input[0] == '@') // VARIABLE
            {
                input = input[1..];
                var variableName = LineParser.ParseToken(ref input);

                setting.InputMode = SettingInputMode.Variable;
                setting.InputVariableName = variableName;
                setting.InterpolatedSetting = param switch
                {
                    StringParameter x => new InterpolatedStringSetting() { MultiLine = x.MultiLine },
                    ListOfStringsParameter _ => new InterpolatedListOfStringsSetting(),
                    DictionaryOfStringsParameter _ => new InterpolatedDictionaryOfStringsSetting(),
                    _ => null
                };
                setting.FixedSetting = param switch // Initialize fixed setting as well, used for type switching
                {
                    BoolParameter _ => new BoolSetting(),
                    IntParameter _ => new IntSetting(),
                    FloatParameter _ => new FloatSetting(),
                    StringParameter x => new StringSetting() { MultiLine = x.MultiLine },
                    ListOfStringsParameter _ => new ListOfStringsSetting(),
                    DictionaryOfStringsParameter _ => new DictionaryOfStringsSetting(),
                    ByteArrayParameter _ => new ByteArraySetting(),
                    EnumParameter x => new EnumSetting() { EnumType = x.EnumType },
                    _ => throw new NotSupportedException()
                };
            }
            else if (input[0] == '$') // INTERPOLATED
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
                    StringParameter x => new StringSetting { Value = (setting.InterpolatedSetting as InterpolatedStringSetting).Value, MultiLine = x.MultiLine },
                    ListOfStringsParameter _ => new ListOfStringsSetting { Value = (setting.InterpolatedSetting as InterpolatedListOfStringsSetting).Value },
                    DictionaryOfStringsParameter _ => new DictionaryOfStringsSetting { Value = (setting.InterpolatedSetting as InterpolatedDictionaryOfStringsSetting).Value },
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
                    EnumParameter x => new EnumSetting { EnumType = x.EnumType, Value = LineParser.ParseToken(ref input) },
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
            => Regex.IsMatch(input, "^( |\t)+([0-9A-Za-z]+) = .+$");

        /// <summary>
        /// Detects the type of the next token in the given <paramref name="input"/>.
        /// If the token was a variable name, it returns null.
        /// </summary>
        public static VariableType? DetectTokenType(string input)
        {
            if (input.StartsWith('"'))
                return VariableType.String;

            if (Regex.IsMatch(input, "^([Tt][Rr][Uu][Ee])|([Ff][Aa][Ll][Ss][Ee])( |$)"))
                return VariableType.Bool;

            if (Regex.IsMatch(input, "^-?[0-9]+( |$)"))
                return VariableType.Int;

            if (Regex.IsMatch(input, "^-?[0-9\\.]+( |$)"))
                return VariableType.Float;

            if (input.StartsWith('['))
                return VariableType.ListOfStrings;

            if (input.StartsWith('{'))
                return VariableType.DictionaryOfStrings;

            if (Regex.IsMatch(input, "^[A-Za-z][A-Za-z0-9]*"))
                return null;

            if (Regex.IsMatch(input, "^[A-Za-z0-9+/=]+"))
                return VariableType.ByteArray;

            throw new Exception("Could not detect the token type");
        }

        /// <summary>
        /// All the supported key identifiers.
        /// </summary>
        public static readonly string[] keyIdentifiers = new[] { "BOOLKEY", "STRINGKEY", "INTKEY", "FLOATKEY", "LISTKEY", "DICTKEY" };

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
            ParseSettingValue(ref line, key.Left, new BoolParameter());
            key.Comparison = Enum.Parse<BoolComparison>(LineParser.ParseToken(ref line));
            ParseSettingValue(ref line, key.Right, new BoolParameter());
            return key;
        }

        private static StringKey ParseStringKey(ref string line)
        {
            var key = new StringKey();
            ParseSettingValue(ref line, key.Left, new StringParameter());
            key.Comparison = Enum.Parse<StrComparison>(LineParser.ParseToken(ref line));
            ParseSettingValue(ref line, key.Right, new StringParameter());
            return key;
        }

        private static IntKey ParseIntKey(ref string line)
        {
            var key = new IntKey();
            ParseSettingValue(ref line, key.Left, new IntParameter());
            key.Comparison = Enum.Parse<NumComparison>(LineParser.ParseToken(ref line));
            ParseSettingValue(ref line, key.Right, new IntParameter());
            return key;
        }

        private static FloatKey ParseFloatKey(ref string line)
        {
            var key = new FloatKey();
            ParseSettingValue(ref line, key.Left, new FloatParameter());
            key.Comparison = Enum.Parse<NumComparison>(LineParser.ParseToken(ref line));
            ParseSettingValue(ref line, key.Right, new FloatParameter());
            return key;
        }

        private static ListKey ParseListKey(ref string line)
        {
            var key = new ListKey();
            ParseSettingValue(ref line, key.Left, new ListOfStringsParameter());
            key.Comparison = Enum.Parse<ListComparison>(LineParser.ParseToken(ref line));
            ParseSettingValue(ref line, key.Right, new StringParameter());
            return key;
        }

        private static DictionaryKey ParseDictKey(ref string line)
        {
            var key = new DictionaryKey();
            ParseSettingValue(ref line, key.Left, new DictionaryOfStringsParameter());
            key.Comparison = Enum.Parse<DictComparison>(LineParser.ParseToken(ref line));
            ParseSettingValue(ref line, key.Right, new StringParameter());
            return key;
        }
    }
}
