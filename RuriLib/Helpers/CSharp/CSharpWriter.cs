using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

namespace RuriLib.Helpers.CSharp;

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
        ArgumentNullException.ThrowIfNull(setting);

        if (setting.InputMode == SettingInputMode.Variable)
        {
            // ExpandoObject members are resolved dynamically, so extension-method syntax like
            // globals.foo.AsString() will fail at runtime. Route those conversions through the
            // static helper instead of forcing a cast on the generated member access.
            if (setting.InputVariableName.StartsWith("globals.") || setting.InputVariableName.StartsWith("input."))
            {
                return GetDynamicCasting(setting);
            }

            return $"{setting.InputVariableName}{GetCasting(setting)}";
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
    /// Converts a <paramref name="setting"/> to a Roslyn expression.
    /// </summary>
    public static ExpressionSyntax FromSettingSyntax(BlockSetting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);

        if (setting.InputMode == SettingInputMode.Variable)
        {
            return GetVariableExpression(setting);
        }

        if (setting.InputMode == SettingInputMode.Interpolated)
        {
            return setting.InterpolatedSetting switch
            {
                InterpolatedStringSetting x => SyntaxFactory.ParseExpression(SerializeInterpString(x.Value)),
                InterpolatedListOfStringsSetting x => SyntaxFactory.ParseExpression(SerializeList(x.Value, true)),
                InterpolatedDictionaryOfStringsSetting x => SyntaxFactory.ParseExpression(SerializeDictionary(x.Value, true)),
                _ => throw new NotImplementedException()
            };
        }

        return setting.FixedSetting switch
        {
            BoolSetting x => x.Value
                ? SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)
                : SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression),
            ByteArraySetting x => SyntaxFactory.ParseExpression(SerializeByteArray(x.Value)),
            DictionaryOfStringsSetting x => SyntaxFactory.ParseExpression(SerializeDictionary(x.Value)),
            FloatSetting x => SyntaxFactory.ParseExpression(ToPrimitive(x.Value)),
            IntSetting x => SyntaxFactory.ParseExpression(ToPrimitive(x.Value)),
            ListOfStringsSetting x => SyntaxFactory.ParseExpression(SerializeList(x.Value)),
            StringSetting x => x.Value is null
                ? SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                : SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(x.Value)),
            EnumSetting x => SyntaxFactory.ParseExpression($"{x.EnumType.FullName}.{x.Value}"),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// Converts a <paramref name="value"/> to a C# primitive.
    /// </summary>
    public static string ToPrimitive(object? value)
    {
        using var writer = new StringWriter();
        using var provider = CodeDomProvider.CreateProvider("CSharp");

        provider.GenerateCodeFromExpression(new CodePrimitiveExpression(value), writer, codeGenOptions);
        return writer.ToString();
    }

    /// <summary>
    /// Serializes a literal without splitting it on multiple lines like <see cref="ToPrimitive(object)"/> does..
    /// </summary>
    public static string SerializeString(string? value)
        => $"\"{(value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";

    /// <summary>
    /// Serializes an interpolated string where &lt;var&gt; is a variable (and sanitizes the '{' and '}' characters).
    /// </summary>
    public static string SerializeInterpString(string? value)
    {
        value ??= string.Empty;
        var segments = new List<(string? Literal, string? Expression)>();
        var literal = new StringBuilder();

        for (var i = 0; i < value.Length;)
        {
            if (i + 1 < value.Length && value[i] == '<' && value[i + 1] == '<')
            {
                literal.Append('<');
                i += 2;
                continue;
            }

            if (i + 1 < value.Length && value[i] == '>' && value[i + 1] == '>')
            {
                literal.Append('>');
                i += 2;
                continue;
            }

            if (value[i] == '<')
            {
                var closingIndex = value.IndexOf('>', i + 1);

                // Keep unmatched or empty angle brackets as literal text.
                if (closingIndex == -1 || closingIndex == i + 1)
                {
                    literal.Append('<');
                    i++;
                    continue;
                }

                if (literal.Length > 0)
                {
                    segments.Add((literal.ToString(), null));
                    literal.Clear();
                }

                segments.Add((null, value[(i + 1)..closingIndex]));
                i = closingIndex + 1;
                continue;
            }

            literal.Append(value[i]);
            i++;
        }

        if (literal.Length > 0)
        {
            segments.Add((literal.ToString(), null));
        }

        var sb = new StringBuilder("$\"");

        foreach (var segment in segments)
        {
            if (segment.Expression is not null)
            {
                sb.Append('{').Append(segment.Expression).Append('}');
            }
            else
            {
                sb.Append(segment.Literal!
                    .Replace("\\", "\\\\")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n")
                    .Replace("\"", "\\\"")
                    .Replace("{", "{{")
                    .Replace("}", "}}"));
            }
        }

        sb.Append('"');
        return sb.ToString();
    }

    /// <summary>
    /// Serializes a byte array.
    /// </summary>
    public static string SerializeByteArray(byte[]? bytes)
    {
        if (bytes is null)
        {
            return "null";
        }

        using var writer = new StringWriter();
        writer.Write("new byte[] {");
        writer.Write(string.Join(", ", bytes.Select(b => Convert.ToInt32(b).ToString())));
        writer.Write("}");
        return writer.ToString();
    }

    /// <summary>
    /// Serializes a list of strings, optionally interpolated.
    /// </summary>
    public static string SerializeList(List<string>? list, bool interpolated = false)
    {
        if (list is null)
        {
            return "null";
        }

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
    public static string SerializeDictionary(Dictionary<string, string>? dict, bool interpolated = false)
    {
        if (dict is null)
        {
            return "null";
        }

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
        var method = $"{GetCastingMethodName(setting)}()";

        // E.g. .DynamicAsString() for dynamics, .AsString() for normal types
        return dynamic ? $".Dynamic{method}" : $".{method}";
    }

    private static string GetDynamicCasting(BlockSetting setting)
    {
        if (setting.FixedSetting is null)
        {
            throw new ArgumentNullException(nameof(setting));
        }

        return $"ObjectExtensions.Dynamic{GetCastingMethodName(setting)}({setting.InputVariableName})";
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

    /// <summary>
    /// Converts a <paramref name="key"/> to a Roslyn expression.
    /// </summary>
    public static ExpressionSyntax ConvertKeySyntax(Key key)
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

        return SyntaxFactory.InvocationExpression(
            SyntaxFactory.IdentifierName("CheckCondition"),
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
            [
                SyntaxFactory.Argument(SyntaxFactory.IdentifierName("data")),
                SyntaxFactory.Argument(FromSettingSyntax(key.Left)),
                SyntaxFactory.Argument(SyntaxFactory.ParseExpression(comparison)),
                SyntaxFactory.Argument(FromSettingSyntax(key.Right))
            ])));
    }

    private static string GetCastingMethodName(BlockSetting setting)
    {
        if (setting.FixedSetting is null)
        {
            throw new ArgumentNullException(nameof(setting));
        }

        return setting.FixedSetting switch
        {
            BoolSetting _ => "AsBool",
            ByteArraySetting _ => "AsBytes",
            DictionaryOfStringsSetting _ => "AsDict",
            FloatSetting _ => "AsFloat",
            IntSetting _ => "AsInt",
            ListOfStringsSetting _ => "AsList",
            StringSetting _ => "AsString",
            _ => throw new NotImplementedException()
        };
    }

    private static ExpressionSyntax GetVariableExpression(BlockSetting setting)
    {
        var variableExpression = SyntaxFactory.ParseExpression(setting.InputVariableName);

        // ExpandoObject members are resolved dynamically, so extension-method syntax like
        // globals.foo.AsString() will fail at runtime. Route those conversions through the
        // static helper instead of forcing a cast on the generated member access.
        if (setting.InputVariableName.StartsWith("globals.") || setting.InputVariableName.StartsWith("input."))
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.ParseExpression($"ObjectExtensions.Dynamic{GetCastingMethodName(setting)}"),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(variableExpression))));
        }

        return SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                variableExpression,
                SyntaxFactory.IdentifierName(GetCastingMethodName(setting))),
            SyntaxFactory.ArgumentList());
    }
}
