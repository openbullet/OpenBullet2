using System.IO;
using System;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Parameters;
using Microsoft.CodeAnalysis.CSharp;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using RuriLib.Models.Blocks.Settings.Interpolated;

namespace RuriLib.Helpers.LoliCode
{
    /// <summary>
    /// Has methods to write LoliCode syntax.
    /// </summary>
    public class LoliCodeWriter : StringWriter
    {
        /// <summary>
        /// Initializes a new <see cref="LoliCodeWriter"/> without any initial value.
        /// </summary>
        public LoliCodeWriter() 
        {

        }

        /// <summary>
        /// Initializes a new <see cref="LoliCodeWriter"/> with an <paramref name="initialValue"/>.
        /// </summary>
        public LoliCodeWriter(string initialValue)
        {
            Write(initialValue);
        }

        /// <summary>
        /// Appends a generic LoliCode token.
        /// </summary>
        public LoliCodeWriter AppendToken(string token, int spaces = 0)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            if (!token.EndsWith(' '))
                token += ' ';

            Write(token.PadLeft(token.Length + spaces));
            return this;
        }

        /// <summary>
        /// Appends a <paramref name="line"/> prefixed by a given number of <paramref name="spaces"/>
        /// and a return character.
        /// </summary>
        public LoliCodeWriter AppendLine(string line = "", int spaces = 0)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            WriteLine(line.PadLeft(line.Length + spaces));
            return this;
        }

        /// <summary>
        /// Appends a <paramref name="setting"/> in the form <code>SettingName = SettingValue</code>.
        /// The setting will be written only if the value is different from the default value in the
        /// corresponding <paramref name="parameter"/>.
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="parameter"></param>
        public LoliCodeWriter AppendSetting(BlockSetting setting, BlockParameter parameter = null,
            int spaces = 2, bool printDefaults = false)
        {
            if (parameter == null)
            {
                AppendLine($"{setting.Name} = {GetSettingValue(setting)}", spaces);
                return this;
            }

            var isDefaultValue = setting.FixedSetting switch
            {
                StringSetting x => x.Value == (parameter as StringParameter).DefaultValue,
                IntSetting x => x.Value == (parameter as IntParameter).DefaultValue,
                FloatSetting x => x.Value == (parameter as FloatParameter).DefaultValue,
                BoolSetting x => x.Value == (parameter as BoolParameter).DefaultValue,
                ByteArraySetting x => Compare(x.Value, (parameter as ByteArrayParameter).DefaultValue),
                ListOfStringsSetting x => Compare(x.Value, (parameter as ListOfStringsParameter).DefaultValue),
                DictionaryOfStringsSetting x => 
                    Compare(x.Value?.Keys, (parameter as DictionaryOfStringsParameter).DefaultValue?.Keys) &&
                    Compare(x.Value?.Values, (parameter as DictionaryOfStringsParameter).DefaultValue?.Values),
                EnumSetting x => x.Value == (parameter as EnumParameter).DefaultValue,
                _ => throw new NotImplementedException(),
            };

            if (setting.InputMode != SettingInputMode.Fixed || !isDefaultValue || printDefaults)
                AppendLine($"{parameter.Name} = {GetSettingValue(setting)}", spaces);

            return this;
        }

        private static bool Compare<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first is null || second is null)
            {
                return first == second;
            }

            return first.SequenceEqual(second);
        }

        /// <summary>
        /// Gets the snippet of the value of a <paramref name="setting"/> in LoliCode syntax.
        /// </summary>
        public static string GetSettingValue(BlockSetting setting)
        {
            // TODO: Make a valid variable name if it's invalid
            if (setting.InputMode == SettingInputMode.Variable)
                return $"@{setting.InputVariableName}";

            if (setting.InputMode == SettingInputMode.Interpolated)
            {
                return '$' + setting.InterpolatedSetting switch
                {
                    InterpolatedStringSetting x => ToLiteral(x.Value),
                    InterpolatedListOfStringsSetting x => SerializeList(x.Value),
                    InterpolatedDictionaryOfStringsSetting x => SerializeDictionary(x.Value),
                    _ => throw new NotImplementedException()
                };
            }

            return setting.FixedSetting switch
            {
                StringSetting x => x.Value == null ? "\"\"" : ToLiteral(x.Value),
                IntSetting x => x.Value.ToString(),
                FloatSetting x => FormatFloat(x.Value),
                BoolSetting x => x.Value.ToString(),
                ByteArraySetting x => x.Value == null ? "" : Convert.ToBase64String(x.Value),
                ListOfStringsSetting x => x.Value == null ? "[]" : SerializeList(x.Value),
                DictionaryOfStringsSetting x => x.Value == null ? "{}" : SerializeDictionary(x.Value),
                EnumSetting x => x.Value,
                _ => throw new NotImplementedException(),
            };
        }

        private static string ToLiteral(string input)
        {
            input ??= string.Empty;

            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(input)).ToFullString();
        }

        private static string FormatFloat(float input)
            => input.ToString(CultureInfo.InvariantCulture);

        private static string SerializeList(List<string> input)
        {
            if (input == null)
                return "[]";

            return "[" + string.Join(", ", input.Select(ToLiteral)) + "]";
        }

        private static string SerializeDictionary(Dictionary<string, string> input) 
        {
            if (input == null || input.Keys.Count == 0)
                return "{}";

            var list = input.Select(kvp => $"{ToLiteral(kvp.Key)}, {ToLiteral(kvp.Value)}");
            return "{" + string.Join(", ", list.Select(s => $"({s})")) + "}";
        }
    }
}
