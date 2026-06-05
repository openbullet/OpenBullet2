using System;
using RuriLib.Helpers.Blocks;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using RuriLib.Models.Variables;
using Xunit;

namespace RuriLib.Tests.Helpers.LoliCode;

public class LoliCodeParserTests
{
    [Fact]
    public void ParseLiteral_NormalLiteral_Parse()
    {
        var input = "\"hello\" how are you";
        Assert.Equal("hello", LineParser.ParseLiteral(ref input));
        Assert.Equal("how are you", input);
    }

    [Fact]
    public void ParseLiteral_LiteralWithEscapedQuotes_Parse()
    {
        var input = "\"I \\\"escape\\\" quotes\"";
        Assert.Equal("I \"escape\" quotes", LineParser.ParseLiteral(ref input));
    }

    [Fact]
    public void ParseLiteral_LiteralWithDoubleEscaping_Parse()
    {
        var input = "\"I \\\\\"don't\\\\\" escape quotes\"";
        Assert.Equal("I \\", LineParser.ParseLiteral(ref input));
    }

    [Fact]
    public void ParseLiteral_NonLiteralPrefix_Throws()
    {
        var input = "x \"hello\"";
        Assert.Throws<Exception>(() => LineParser.ParseLiteral(ref input));
    }

    [Fact]
    public void ParseLiteral_WriterEscapesRoundTrip_Parse()
    {
        var expected = "line\nbreak\tquote\"slash\\";
        var input = LoliCodeWriter.GetSettingValue(new BlockSetting
        {
            InputMode = SettingInputMode.Fixed,
            FixedSetting = new StringSetting { Value = expected }
        });

        Assert.Equal(expected, LineParser.ParseLiteral(ref input));
        Assert.Equal(string.Empty, input);
    }

    [Fact]
    public void ParseLiteral_ControlCharacterEscapesRoundTrip_Parse()
    {
        var expected = "\0\a\b\f\n\r\t\v";
        var input = LoliCodeWriter.GetSettingValue(new BlockSetting
        {
            InputMode = SettingInputMode.Fixed,
            FixedSetting = new StringSetting { Value = expected }
        });

        Assert.Equal(expected, LineParser.ParseLiteral(ref input));
        Assert.Equal(string.Empty, input);
    }

    [Fact]
    public void ParseLiteral_InvalidEscape_Throws()
    {
        var input = "\"\\q\"";
        Assert.Throws<Exception>(() => LineParser.ParseLiteral(ref input));
    }

    [Fact]
    public void ParseInt_NormalInt_Parse()
    {
        var input = "42 is the answer";
        Assert.Equal(42, LineParser.ParseInt(ref input));
        Assert.Equal("is the answer", input);
    }

    [Fact]
    public void ParseInt_NonIntPrefix_Throws()
    {
        var input = "abc 42";
        Assert.Throws<Exception>(() => LineParser.ParseInt(ref input));
    }

    [Fact]
    public void ParseFloat_NormalFloat_Parse()
    {
        var input = "3.14 is not pi";
        Assert.Equal(3.14f, LineParser.ParseFloat(ref input));
        Assert.Equal("is not pi", input);
    }

    [Fact]
    public void ParseFloat_NonFloatPrefix_Throws()
    {
        var input = "abc 3.14";
        Assert.Throws<Exception>(() => LineParser.ParseFloat(ref input));
    }

    [Fact]
    public void ParseToken_TabSeparated_Parse()
    {
        var input = "token\tremaining";
        Assert.Equal("token", LineParser.ParseToken(ref input));
        Assert.Equal("remaining", input);
    }

    [Fact]
    public void ParseToken_NewlineSeparated_Parse()
    {
        var input = "token\nremaining";
        Assert.Equal("token", LineParser.ParseToken(ref input));
        Assert.Equal("remaining", input);
    }

    [Fact]
    public void ParseList_EmptyList_Parse()
    {
        var input = "[] such emptiness";
        Assert.Empty(LineParser.ParseList(ref input));
        Assert.Equal("such emptiness", input);
    }

    [Fact]
    public void ParseList_EmptyListWithSpacesInside_Parse()
    {
        var input = "[ ]";
        Assert.Empty(LineParser.ParseList(ref input));
    }

    [Fact]
    public void ParseList_SingleElementList_Parse()
    {
        var input = "[\"one\"]";
        Assert.Equal(new[] { "one" }, LineParser.ParseList(ref input).ToArray());
    }

    [Fact]
    public void ParseList_MultiElementList_Parse()
    {
        var input = "[\"one\", \"two\", \"three\"]";
        Assert.Equal(new[] { "one", "two", "three" }, LineParser.ParseList(ref input).ToArray());
    }

    [Fact]
    public void ParseList_MissingCommaBetweenItems_Throws()
    {
        var input = "[\"one\" \"two\"]";
        Assert.Throws<Exception>(() => LineParser.ParseList(ref input));
    }

    [Fact]
    public void ParseList_UnterminatedList_Throws()
    {
        var input = "[\"one\"";
        Assert.Throws<Exception>(() => LineParser.ParseList(ref input));
    }

    [Fact]
    public void ParseByteArray_NormalBase64_Parse()
    {
        var input = "/wA= is my byte array";
        Assert.Equal([0xFF, 0x00], LineParser.ParseByteArray(ref input));
        Assert.Equal("is my byte array", input);
    }

    [Fact]
    public void ParseByteArray_NonBase64Prefix_Throws()
    {
        var input = "* /wA=";
        Assert.Throws<Exception>(() => LineParser.ParseByteArray(ref input));
    }

    [Fact]
    public void ParseByteArray_InvalidLength_ThrowsWithoutConsumingInput()
    {
        var input = "abc remaining";
        Assert.Throws<Exception>(() => LineParser.ParseByteArray(ref input));
        Assert.Equal("abc remaining", input);
    }

    [Fact]
    public void ParseByteArray_InvalidPadding_ThrowsWithoutConsumingInput()
    {
        var input = "ab=c remaining";
        Assert.Throws<Exception>(() => LineParser.ParseByteArray(ref input));
        Assert.Equal("ab=c remaining", input);
    }

    [Fact]
    public void ParseBool_NormalBool_Parse()
    {
        var input = "True indeed";
        Assert.True(LineParser.ParseBool(ref input));
        Assert.Equal("indeed", input);
    }

    [Fact]
    public void ParseBool_PrefixedToken_Throws()
    {
        var input = "TrueValue";
        Assert.Throws<Exception>(() => LineParser.ParseBool(ref input));
    }

    [Fact]
    public void ParseDictionary_EmptyDictionary_Parse()
    {
        var input = "{} such emptiness";
        Assert.Empty(LineParser.ParseDictionary(ref input));
        Assert.Equal("such emptiness", input);
    }

    [Fact]
    public void ParseDictionary_EmptyDictionaryWithSpacesInside_Parse()
    {
        var input = "{ }";
        Assert.Empty(LineParser.ParseDictionary(ref input));
    }

    [Fact]
    public void ParseDictionary_SingleEntry_Parse()
    {
        var input = "{ (\"key1\", \"value1\") }";
        var dict = LineParser.ParseDictionary(ref input);
        Assert.Equal("value1", dict["key1"]);
    }

    [Fact]
    public void ParseDictionary_MultiEntry_Parse()
    {
        var input = "{ (\"key1\", \"value1\"), (\"key2\", \"value2\") }";
        var dict = LineParser.ParseDictionary(ref input);
        Assert.Equal("value1", dict["key1"]);
        Assert.Equal("value2", dict["key2"]);
    }

    [Fact]
    public void ParseDictionary_MissingCommaBetweenEntries_Throws()
    {
        var input = "{ (\"key1\", \"value1\") (\"key2\", \"value2\") }";
        Assert.Throws<Exception>(() => LineParser.ParseDictionary(ref input));
    }

    [Fact]
    public void ParseDictionary_UnterminatedDictionary_Throws()
    {
        var input = "{ (\"key1\", \"value1\")";
        Assert.Throws<Exception>(() => LineParser.ParseDictionary(ref input));
    }

    [Fact]
    public void ParseSetting_FixedStringSetting_Parse()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
        var valueSetting = block.Settings["value"];

        var input = "value = \"myValue\"";
        LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
        Assert.Equal(SettingInputMode.Fixed, valueSetting.InputMode);
        Assert.IsType<StringSetting>(valueSetting.FixedSetting);
        Assert.Equal("myValue", ((StringSetting)valueSetting.FixedSetting!).Value);
    }

    [Fact]
    public void ParseSetting_Variable_Parse()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
        var valueSetting = block.Settings["value"];

        var input = "value = @myVariable";
        LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
        Assert.Equal(SettingInputMode.Variable, valueSetting.InputMode);
        Assert.Equal("myVariable", valueSetting.InputVariableName);
    }

    [Fact]
    public void ParseSetting_VariableFollowedByNewline_KeepsEmptyVariableName()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
        var valueSetting = block.Settings["value"];

        var input = "value = @\nnext";
        LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
        Assert.Equal(SettingInputMode.Variable, valueSetting.InputMode);
        Assert.Equal(string.Empty, valueSetting.InputVariableName);
        Assert.Equal("\nnext", input);
    }

    [Fact]
    public void ParseSetting_InterpolatedString_Parse()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
        var valueSetting = block.Settings["value"];

        var input = "value = $\"my <interp> string\"";
        LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
        Assert.Equal(SettingInputMode.Interpolated, valueSetting.InputMode);
        Assert.IsType<InterpolatedStringSetting>(valueSetting.InterpolatedSetting);
        Assert.Equal("my <interp> string", ((InterpolatedStringSetting)valueSetting.InterpolatedSetting!).Value);
    }

    [Fact]
    public void ParseSetting_InterpolatedStringWithEscapedAngleBrackets_PreservesRawValue()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
        var valueSetting = block.Settings["value"];

        var input = "value = $\"hello <<name>>\"";
        LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
        Assert.Equal(SettingInputMode.Interpolated, valueSetting.InputMode);
        Assert.Equal("hello <<name>>", ((InterpolatedStringSetting)valueSetting.InterpolatedSetting!).Value);
    }

    [Fact]
    public void ParseSetting_InterpolatedStringWithTripleAngleBrackets_PreservesRawValue()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
        var valueSetting = block.Settings["value"];

        var input = "value = $\"hello <<<name>>>\"";
        LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor);
        Assert.Equal(SettingInputMode.Interpolated, valueSetting.InputMode);
        Assert.Equal("hello <<<name>>>", ((InterpolatedStringSetting)valueSetting.InterpolatedSetting!).Value);
    }

    [Fact]
    public void ParseSetting_MissingEquals_Throws()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
        var input = "value";

        Assert.Throws<Exception>(() => LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor));
    }

    [Fact]
    public void IsSetting_WithoutIndentation_ReturnsTrue()
    {
        Assert.True(LoliCodeParser.IsSetting("value = \"hello\""));
    }

    [Fact]
    public void IsSetting_WithoutSpacesAroundEquals_ReturnsTrue()
    {
        Assert.True(LoliCodeParser.IsSetting("  value=\"hello\""));
    }

    [Fact]
    public void IsSetting_WithTabsAroundEquals_ReturnsTrue()
    {
        Assert.True(LoliCodeParser.IsSetting("\tvalue\t=\t@variable"));
    }

    [Fact]
    public void IsSetting_WithMissingValue_ReturnsFalse()
    {
        Assert.False(LoliCodeParser.IsSetting("value =   "));
    }

    [Fact]
    public void DetectTokenType_ValidBool_ReturnsBool()
    {
        Assert.Equal(VariableType.Bool, LoliCodeParser.DetectTokenType("True"));
    }

    [Fact]
    public void DetectTokenType_BoolPrefixIdentifier_ReturnsNull()
    {
        Assert.Null(LoliCodeParser.DetectTokenType("TrueValue"));
    }

    [Fact]
    public void DetectTokenType_ValidFloat_ReturnsFloat()
    {
        Assert.Equal(VariableType.Float, LoliCodeParser.DetectTokenType("-1.5"));
    }

    [Fact]
    public void DetectTokenType_InvalidFloat_Throws()
    {
        Assert.Throws<Exception>(() => LoliCodeParser.DetectTokenType("1..2"));
    }

    [Fact]
    public void DetectTokenType_Base64Token_ReturnsByteArray()
    {
        Assert.Equal(VariableType.ByteArray, LoliCodeParser.DetectTokenType("/wA="));
    }
}
