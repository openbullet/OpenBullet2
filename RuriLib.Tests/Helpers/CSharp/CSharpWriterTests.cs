using Microsoft.CodeAnalysis;
using RuriLib.Helpers.CSharp;
using RuriLib.Models.Blocks.Settings;
using Xunit;

namespace RuriLib.Tests.Helpers.CSharp;

public class CSharpWriterTests
{
    [Fact]
    public void SerializeInterpString_Alone_ReplaceCorrectly()
    {
        Assert.Equal("$\"{value}\"", CSharpWriter.SerializeInterpString("<value>"));
    }

    [Fact]
    public void SerializeInterpString_Surrounded_ReplaceCorrectly()
    {
        Assert.Equal("$\"my {value} is cool\"", CSharpWriter.SerializeInterpString("my <value> is cool"));
    }

    [Fact]
    public void SerializeInterpString_SingleCharacter_ReplaceCorrectly()
    {
        Assert.Equal("$\"my {a} is cool\"", CSharpWriter.SerializeInterpString("my <a> is cool"));
    }

    [Fact]
    public void SerializeInterpString_EscapedAngleBrackets_RemainLiteral()
    {
        Assert.Equal("$\"hello <name>\"", CSharpWriter.SerializeInterpString("hello <<name>>"));
    }

    [Fact]
    public void SerializeInterpString_TripleAngleBrackets_EscapeAndInterpolate()
    {
        Assert.Equal("$\"hello <{name}>\"", CSharpWriter.SerializeInterpString("hello <<<name>>>"));
    }

    [Fact]
    public void SerializeInterpString_StandaloneEscapedAngleBrackets_RemainLiteral()
    {
        Assert.Equal("$\"< and >\"", CSharpWriter.SerializeInterpString("<< and >>"));
    }

    [Fact]
    public void SerializeInterpString_QuadrupleAngleBrackets_RemainLiteral()
    {
        Assert.Equal("$\"<<name>>\"", CSharpWriter.SerializeInterpString("<<<<name>>>>"));
    }

    [Fact]
    public void SerializeInterpString_EmptyAngleBrackets_RemainLiteral()
    {
        Assert.Equal("$\"<>\"", CSharpWriter.SerializeInterpString("<>"));
    }

    [Fact]
    public void SerializeInterpString_Braces_AreEscapedAroundInterpolations()
    {
        Assert.Equal("$\"{{{value}}}\"", CSharpWriter.SerializeInterpString("{<value>}"));
    }

    [Fact]
    public void SerializeInterpString_Newlines_AreEscaped()
    {
        Assert.Equal("$\"first\\r\\nsecond {value}\\nthird\"", CSharpWriter.SerializeInterpString("first\r\nsecond <value>\nthird"));
    }

    [Fact]
    public void SerializeByteArray_Null_ReturnsNullLiteral()
    {
        Assert.Equal("null", CSharpWriter.SerializeByteArray(null));
    }

    [Fact]
    public void SerializeList_Null_ReturnsNullLiteral()
    {
        Assert.Equal("null", CSharpWriter.SerializeList(null));
    }

    [Fact]
    public void SerializeDictionary_Null_ReturnsNullLiteral()
    {
        Assert.Equal("null", CSharpWriter.SerializeDictionary(null));
    }

    [Fact]
    public void ToPrimitive_Null_ReturnsNullLiteral()
    {
        Assert.Equal("null", CSharpWriter.ToPrimitive(null));
    }

    [Fact]
    public void FromSetting_GlobalVariable_UsesDynamicHelperCall()
    {
        var setting = new BlockSetting
        {
            InputMode = SettingInputMode.Variable,
            InputVariableName = "globals.myVar",
            FixedSetting = new StringSetting()
        };

        Assert.Equal("ObjectExtensions.DynamicAsString(globals.myVar)", CSharpWriter.FromSetting(setting));
    }

    [Fact]
    public void FromSetting_InputVariable_UsesDynamicHelperCall()
    {
        var setting = new BlockSetting
        {
            InputMode = SettingInputMode.Variable,
            InputVariableName = "input.count",
            FixedSetting = new IntSetting()
        };

        Assert.Equal("ObjectExtensions.DynamicAsInt(input.count)", CSharpWriter.FromSetting(setting));
    }

    [Fact]
    public void FromSetting_NormalVariable_UsesRegularExtensionCall()
    {
        var setting = new BlockSetting
        {
            InputMode = SettingInputMode.Variable,
            InputVariableName = "myVar",
            FixedSetting = new StringSetting()
        };

        Assert.Equal("myVar.AsString()", CSharpWriter.FromSetting(setting));
    }

    [Fact]
    public void FromSettingSyntax_GlobalVariable_UsesDynamicHelperCall()
    {
        var setting = new BlockSetting
        {
            InputMode = SettingInputMode.Variable,
            InputVariableName = "globals.myVar",
            FixedSetting = new StringSetting()
        };

        Assert.Equal(
            "ObjectExtensions.DynamicAsString(globals.myVar)",
            CSharpWriter.FromSettingSyntax(setting).NormalizeWhitespace().ToFullString());
    }

    [Fact]
    public void FromSettingSyntax_InterpolatedString_UsesInterpolatedExpression()
    {
        var setting = new BlockSetting
        {
            InputMode = SettingInputMode.Interpolated,
            InterpolatedSetting = new RuriLib.Models.Blocks.Settings.Interpolated.InterpolatedStringSetting
            {
                Value = "hello <<<name>>> and <<friend>>"
            }
        };

        Assert.Equal(
            "$\"hello <{name}> and <friend>\"",
            CSharpWriter.FromSettingSyntax(setting).NormalizeWhitespace().ToFullString());
    }
}
