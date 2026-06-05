using RuriLib.Exceptions;
using RuriLib.Helpers.Blocks;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks;
using Xunit;

namespace RuriLib.Tests.Helpers.LoliCode;

public class LoliCodeParserDiagnosticTests
{
    [Fact]
    public void ParseSetting_UnknownSetting_ReportsSettingNameAndBlockId()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
        var input = "  unknown = \"value\"";

        var ex = Assert.Throws<LineParsingException>(
            () => LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor));

        Assert.Equal(3, ex.ColumnNumber);
        Assert.Equal("Unknown setting 'unknown' for block 'ConstantString'", ex.Message);
    }

    [Fact]
    public void ParseSetting_MissingEquals_ReportsSettingNameAndColumn()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
        var input = "  value   ";

        var ex = Assert.Throws<LineParsingException>(
            () => LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor));

        Assert.Equal(11, ex.ColumnNumber);
        Assert.Equal("Expected '=' after setting name 'value'", ex.Message);
    }

    [Fact]
    public void DetectTokenType_EmptyInput_ReportsExpectedToken()
    {
        var ex = Assert.Throws<LineParsingException>(() => LoliCodeParser.DetectTokenType(string.Empty));

        Assert.Equal(1, ex.ColumnNumber);
        Assert.Equal("Expected token", ex.Message);
    }

    [Fact]
    public void DetectTokenType_UnknownToken_ReportsToken()
    {
        var ex = Assert.Throws<LineParsingException>(() => LoliCodeParser.DetectTokenType("1..2"));

        Assert.Equal(1, ex.ColumnNumber);
        Assert.Equal("Could not detect the type of token '1..2'", ex.Message);
    }
}
