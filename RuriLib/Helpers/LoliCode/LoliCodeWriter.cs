using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;

namespace RuriLib.Helpers.LoliCode;

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
        ArgumentNullException.ThrowIfNull(token);

        if (!token.EndsWith(' '))
        {
            token += ' ';
        }

        Write(token.PadLeft(token.Length + spaces));
        return this;
    }

    /// <summary>
    /// Appends a <paramref name="line"/> prefixed by a given number of <paramref name="spaces"/>
    /// and a return character.
    /// </summary>
    public LoliCodeWriter AppendLine(string line = "", int spaces = 0)
    {
        ArgumentNullException.ThrowIfNull(line);

        WriteLine(line.PadLeft(line.Length + spaces));
        return this;
    }

    /// <summary>
    /// Appends a <paramref name="setting"/> in the form <code>SettingName = SettingValue</code>.
    /// The setting will be written only if the value is different from the default value in the
    /// corresponding <paramref name="parameter"/>.
    /// </summary>
    /// <param name="setting">The setting to append.</param>
    /// <param name="parameter">The optional parameter metadata used to detect default values.</param>
    /// <param name="spaces">The indentation width.</param>
    /// <param name="printDefaults">Whether default values should still be written.</param>
    /// <returns>The current writer.</returns>
    public LoliCodeWriter AppendSetting(BlockSetting setting, BlockParameter? parameter = null,
        int spaces = 2, bool printDefaults = false)
    {
        ArgumentNullException.ThrowIfNull(setting);

        if (parameter is null)
        {
            AppendLine($"{setting.Name} = {GetSettingValue(setting)}", spaces);
            return this;
        }

        var isDefaultValue = IsDefaultValue(setting, parameter);

        if (setting.InputMode == SettingInputMode.Variable || !isDefaultValue || printDefaults)
        {
            AppendLine($"{parameter.Name} = {GetSettingValue(setting)}", spaces);
        }

        return this;
    }

    private static bool IsDefaultValue(BlockSetting setting, BlockParameter parameter)
        => setting.InputMode switch
        {
            SettingInputMode.Fixed => setting.FixedSetting switch
            {
                StringSetting x => parameter is StringParameter stringParameter
                    && x.Value == stringParameter.DefaultValue,
                IntSetting x => parameter is IntParameter intParameter
                    && x.Value == intParameter.DefaultValue,
                FloatSetting x => parameter is FloatParameter floatParameter
                    && Math.Abs(x.Value - floatParameter.DefaultValue) < double.Epsilon,
                BoolSetting x => parameter is BoolParameter boolParameter
                    && x.Value == boolParameter.DefaultValue,
                ByteArraySetting x => parameter is ByteArrayParameter byteArrayParameter
                    && Compare(x.Value, byteArrayParameter.DefaultValue),
                ListOfStringsSetting x => parameter is ListOfStringsParameter listParameter
                    && Compare(x.Value, listParameter.DefaultValue),
                DictionaryOfStringsSetting x => parameter is DictionaryOfStringsParameter dictionaryParameter
                    && Compare(x.Value?.Keys, dictionaryParameter.DefaultValue?.Keys)
                    && Compare(x.Value?.Values, dictionaryParameter.DefaultValue?.Values),
                EnumSetting x => parameter is EnumParameter enumParameter
                    && x.Value == enumParameter.DefaultValue,
                _ => throw new NotImplementedException(),
            },
            SettingInputMode.Interpolated => setting.InterpolatedSetting switch
            {
                InterpolatedStringSetting x => parameter is StringParameter stringParameter
                    && x.Value == stringParameter.DefaultValue,
                InterpolatedListOfStringsSetting x => parameter is ListOfStringsParameter listParameter
                    && Compare(x.Value, listParameter.DefaultValue),
                InterpolatedDictionaryOfStringsSetting x => parameter is DictionaryOfStringsParameter dictionaryParameter
                    && Compare(x.Value?.Keys, dictionaryParameter.DefaultValue?.Keys)
                    && Compare(x.Value?.Values, dictionaryParameter.DefaultValue?.Values),
                _ => false
            },
            SettingInputMode.Variable => false,
            _ => false
        };

    private static bool Compare<T>(IEnumerable<T>? first, IEnumerable<T>? second)
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
        ArgumentNullException.ThrowIfNull(setting);

        // Preserve the original reference when serializing. Invalid names should be
        // validated where they are entered instead of being rewritten on save.
        if (setting.InputMode == SettingInputMode.Variable)
        {
            return $"@{setting.InputVariableName}";
        }

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
            StringSetting x => x.Value is null ? "\"\"" : ToLiteral(x.Value),
            IntSetting x => x.Value.ToString(CultureInfo.InvariantCulture),
            FloatSetting x => FormatFloat(x.Value),
            BoolSetting x => x.Value.ToString(),
            ByteArraySetting x => x.Value is null ? string.Empty : Convert.ToBase64String(x.Value),
            ListOfStringsSetting x => x.Value is null ? "[]" : SerializeList(x.Value),
            DictionaryOfStringsSetting x => x.Value is null ? "{}" : SerializeDictionary(x.Value),
            EnumSetting x => x.Value,
            _ => throw new NotImplementedException(),
        };
    }

    private static string ToLiteral(string? input)
        => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(input ?? string.Empty)).ToFullString();

    private static string FormatFloat(double input)
        => input.ToString(CultureInfo.InvariantCulture);

    private static string SerializeList(List<string>? input)
    {
        if (input is null)
        {
            return "[]";
        }

        return "[" + string.Join(", ", input.Select(ToLiteral)) + "]";
    }

    private static string SerializeDictionary(Dictionary<string, string>? input)
    {
        if (input is null || input.Count == 0)
        {
            return "{}";
        }

        var list = input.Select(kvp => $"{ToLiteral(kvp.Key)}, {ToLiteral(kvp.Value)}");
        return "{" + string.Join(", ", list.Select(s => $"({s})")) + "}";
    }
}
