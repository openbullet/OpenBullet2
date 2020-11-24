using System.IO;
using System;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Parameters;
using Microsoft.CodeAnalysis.CSharp;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using RuriLib.Models.Blocks.Settings.Interpolated;

namespace RuriLib.Helpers.LoliCode
{
    public class LoliCodeWriter : StringWriter
    {
        public LoliCodeWriter() { }
        public LoliCodeWriter(string initialValue)
        {
            Write(initialValue);
        }

        public LoliCodeWriter AppendToken(string token, int spaces = 0)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            if (!token.EndsWith(' '))
                token += ' ';

            Write(token.PadLeft(token.Length + spaces));
            return this;
        }

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
        public LoliCodeWriter AppendSetting(BlockSetting setting, BlockParameter parameter = null, int spaces = 2)
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
                ByteArraySetting x => x.Value == (parameter as ByteArrayParameter).DefaultValue,
                ListOfStringsSetting x => x.Value == (parameter as ListOfStringsParameter).DefaultValue,
                DictionaryOfStringsSetting x => x.Value == (parameter as DictionaryOfStringsParameter).DefaultValue,
                EnumSetting x => x.Value == (parameter as EnumParameter).DefaultValue,
                _ => throw new NotImplementedException(),
            };

            if (setting.InputMode != SettingInputMode.Fixed || !isDefaultValue)
                AppendLine($"{parameter.Name} = {GetSettingValue(setting)}", spaces);

            return this;
        }

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
                StringSetting x => ToLiteral(x.Value),
                IntSetting x => x.Value.ToString(),
                FloatSetting x => FormatFloat(x.Value),
                BoolSetting x => x.Value.ToString(),
                ByteArraySetting x => Convert.ToBase64String(x.Value),
                ListOfStringsSetting x => SerializeList(x.Value),
                DictionaryOfStringsSetting x => SerializeDictionary(x.Value),
                EnumSetting x => x.Value,
                _ => throw new NotImplementedException(),
            };
        }

        public static string ToLiteral(string input)
        {
            if (input == null)
                input = string.Empty;

            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(input)).ToFullString();
        }

        public static string FormatFloat(float input)
            => input.ToString(CultureInfo.InvariantCulture);

        public static string SerializeList(List<string> input)
        {
            if (input == null)
                return "[]";

            return "[" + string.Join(", ", input.Select(i => ToLiteral(i))) + "]";
        }

        public static string SerializeDictionary(Dictionary<string, string> input) 
        {
            if (input == null || input.Keys.Count == 0)
                return "{}";

            var list = input.Select(kvp => $"{ToLiteral(kvp.Key)}, {ToLiteral(kvp.Value)}");
            return "{" + string.Join(", ", list.Select(s => $"({s})")) + "}";
        }
    }
}
