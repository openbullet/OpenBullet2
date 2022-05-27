using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RuriLib.Helpers.CSharp
{
    /// <summary>
    /// In charge of writing C# snippets that can be executed.
    /// </summary>
    public class CSharpWriter
    {
        private static readonly CodeGeneratorOptions codeGenOptions = new()
        {
            BlankLinesBetweenMembers = false
        };

        /// <summary>
        /// Converts a <paramref name="setting"/> to a valid C# snippet.
        /// </summary>
        public static string FromSetting(BlockSetting setting)
        {
            if (setting.InputMode == SettingInputMode.Variable)
            {
                // TODO: Find a better way to cast things from an ExpandoObject

                // If it's a variable from an ExpandoObject we need to hard cast it, otherwise we get
                // a runtime exception when trying to use extension methods on it.
                if (setting.InputVariableName.StartsWith("globals.") || setting.InputVariableName.StartsWith("input."))
                {
                    return $"({GetTypeName(setting)})((object){setting.InputVariableName}){GetCasting(setting, true)}";
                }
                else
                {
                    return $"{setting.InputVariableName}{GetCasting(setting)}";
                }
            }

            if (setting.InputMode == SettingInputMode.Interpolated)
            {
                return setting.InterpolatedSetting switch
                {
                    InterpolatedStringSetting x => SerializeInterpString(x.Value),
                    InterpolatedListOfStringsSetting x => SerializeList(x.Value, true),
                    InterpolatedDictionaryOfStringsSetting x => SerializeDictionary(x.Value, true),
                    _ => throw new NotImplementedException()
                };
                
            }

            return setting.FixedSetting switch
            {
                BoolSetting x => ToPrimitive(x.Value),
                ByteArraySetting x => SerializeByteArray(x.Value),
                DictionaryOfStringsSetting x => SerializeDictionary(x.Value),
                FloatSetting x => ToPrimitive(x.Value),
                IntSetting x => ToPrimitive(x.Value),
                ListOfStringsSetting x => SerializeList(x.Value),
                StringSetting x => ToPrimitive(x.Value),
                EnumSetting x => $"{x.EnumType.FullName}.{x.Value}",
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Converts a <paramref name="value"/> to a C# primitive.
        /// </summary>
        public static string ToPrimitive(object value)
        {
            using var writer = new StringWriter();
            using var provider = CodeDomProvider.CreateProvider("CSharp");

            provider.GenerateCodeFromExpression(new CodePrimitiveExpression(value), writer, codeGenOptions);
            return writer.ToString();
        }

        /// <summary>
        /// Serializes a literal without splitting it on multiple lines like <see cref="ToPrimitive(object)"/> does..
        /// </summary>
        public static string SerializeString(string value)
            => $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";

        /// <summary>
        /// Serializes an interpolated string where &lt;var&gt; is a variable (and sanitizes the '{' and '}' characters).
        /// </summary>
        public static string SerializeInterpString(string value)
        {
            var sb = new StringBuilder(SerializeString(value))
                .Replace("{", "{{")
                .Replace("}", "}}");

            foreach (Match match in Regex.Matches(value, @"<([^>]+)>"))
                sb.Replace(match.Groups[0].Value.Replace("\\", "\\\\").Replace("\"", "\\\""), '{' + match.Groups[1].Value + '}');

            return '$' + sb.ToString();
        }

        /// <summary>
        /// Serializes a byte array.
        /// </summary>
        public static string SerializeByteArray(byte[] bytes)
        {
            if (bytes == null)
                return "null";

            using var writer = new StringWriter();
            writer.Write("new byte[] {");
            writer.Write(string.Join(", ", bytes.Select(b => Convert.ToInt32(b).ToString())));
            writer.Write("}");
            return writer.ToString();
        }

        /// <summary>
        /// Serializes a list of strings, optionally interpolated.
        /// </summary>
        public static string SerializeList(List<string> list, bool interpolated = false)
        {
            if (list == null)
                return "null";

            using var writer = new StringWriter();
            writer.Write("new List<string> {");

            var toWrite = list.Select(e => interpolated
                ? SerializeInterpString(e)
                : ToPrimitive(e));

            writer.Write(string.Join(", ", toWrite));
            writer.Write("}");
            return writer.ToString();
        }

        /// <summary>
        /// Serializes a dictionary of strings, optionally interpolated.
        /// </summary>
        public static string SerializeDictionary(Dictionary<string, string> dict, bool interpolated = false)
        {
            if (dict == null)
                return "null";

            using var writer = new StringWriter();
            writer.Write("new Dictionary<string, string> {");

            var toWrite = dict.Select(kvp => interpolated
                ? $"{{{SerializeInterpString(kvp.Key)}, {SerializeInterpString(kvp.Value)}}}"
                : $"{{{ToPrimitive(kvp.Key)}, {ToPrimitive(kvp.Value)}}}");

            writer.Write(string.Join(", ", toWrite));
            writer.Write("}");
            return writer.ToString();
        }

        private static string GetCasting(BlockSetting setting, bool dynamic = false)
        {
            if (setting.FixedSetting == null)
                throw new ArgumentNullException(nameof(setting));

            var method = setting.FixedSetting switch
            {
                BoolSetting _ => "AsBool()",
                ByteArraySetting _ => "AsBytes()",
                DictionaryOfStringsSetting _ => "AsDict()",
                FloatSetting _ => "AsFloat()",
                IntSetting _ => "AsInt()",
                ListOfStringsSetting _ => "AsList()",
                StringSetting _ => "AsString()",
                _ => throw new NotImplementedException()
            };

            // E.g. .DynamicAsString() for dynamics, .AsString() for normal types
            return dynamic ? $".Dynamic{method}" : $".{method}";
        }

        private static string GetTypeName(BlockSetting setting)
        {
            if (setting.FixedSetting == null)
                throw new ArgumentNullException(nameof(setting));

            return setting.FixedSetting switch
            {
                BoolSetting _ => "bool",
                ByteArraySetting _ => "byte[]",
                DictionaryOfStringsSetting _ => "Dictionary<string, string>",
                FloatSetting _ => "float",
                IntSetting _ => "int",
                ListOfStringsSetting _ => "List<string>",
                StringSetting _ => "string",
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Converts a <paramref name="key"/> to a valid C# snippet.
        /// </summary>
        public static string ConvertKey(Key key)
        {
            var comparison = key switch
            {
                BoolKey x => $"BoolComparison.{x.Comparison}",
                StringKey x => $"StrComparison.{x.Comparison}",
                IntKey x => $"NumComparison.{x.Comparison}",
                FloatKey x => $"NumComparison.{x.Comparison}",
                ListKey x => $"ListComparison.{x.Comparison}",
                DictionaryKey x => $"DictComparison.{x.Comparison}",
                _ => throw new Exception("Unknown key type")
            };

            var left = FromSetting(key.Left);
            var right = FromSetting(key.Right);

            return $"CheckCondition(data, {left}, {comparison}, {right})";
        }
    }
}
