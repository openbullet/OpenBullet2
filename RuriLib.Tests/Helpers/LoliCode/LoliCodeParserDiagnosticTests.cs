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
    public void ParseSetting_InvalidEnumValue_ReportsValueColumnAndValidValues()
    {
        var block = BlockFactory.GetBlock<BlockInstance>("HttpRequest");
        var input = "method = INVALID";

        var ex = Assert.Throws<LineParsingException>(
            () => LoliCodeParser.ParseSetting(ref input, block.Settings, block.Descriptor));

        Assert.Equal(10, ex.ColumnNumber);
        Assert.Contains("Invalid HttpMethod value 'INVALID'", ex.Message);
        Assert.Contains("GET", ex.Message);
        Assert.Contains("POST", ex.Message);
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

    [Fact]
    public void ParseKey_InvalidKeyType_ReportsValidKeyTypes()
    {
        var line = "@myString Contains \"abc\"";

        var ex = Assert.Throws<LineParsingException>(() => LoliCodeParser.ParseKey(ref line, "NOPE"));

        Assert.Equal(1, ex.ColumnNumber);
        Assert.Equal("Invalid key type 'NOPE'. Valid values: BOOLKEY, STRINGKEY, INTKEY, FLOATKEY, LISTKEY, DICTKEY",
            ex.Message);
    }

    [Fact]
    public void ParseKey_InvalidComparison_ReportsValidValues()
    {
        var line = "@myString BadComparison \"abc\"";

        var ex = Assert.Throws<LineParsingException>(() => LoliCodeParser.ParseKey(ref line, "STRINGKEY"));

        Assert.Equal(1, ex.ColumnNumber);
        Assert.Contains("Invalid StrComparison value 'BadComparison'", ex.Message);
        Assert.Contains("Contains", ex.Message);
    }
}
