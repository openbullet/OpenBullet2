using RuriLib.Exceptions;
using RuriLib.Helpers.LoliCode;
using Xunit;

namespace RuriLib.Tests.Helpers.LoliCode;

public class LineParserDiagnosticTests
{
    [Fact]
    public void ParseLiteral_NonLiteralPrefix_ReportsColumnAndExpectedCharacter()
    {
        var input = "  x \"hello\"";

        var ex = Assert.Throws<LineParsingException>(() => LineParser.ParseLiteral(ref input));

        Assert.Equal(3, ex.ColumnNumber);
        Assert.Equal("Expected '\"' to start a string literal, found 'x'", ex.Message);
    }

    [Fact]
    public void ParseLiteral_InvalidEscape_ReportsEscapeColumn()
    {
        var input = "\"\\q\"";

        var ex = Assert.Throws<LineParsingException>(() => LineParser.ParseLiteral(ref input));

        Assert.Equal(3, ex.ColumnNumber);
        Assert.Equal("Invalid escape sequence '\\q'", ex.Message);
    }

    [Fact]
    public void ParseLiteral_UnterminatedLiteral_ReportsEndColumn()
    {
        var input = "  \"hello";

        var ex = Assert.Throws<LineParsingException>(() => LineParser.ParseLiteral(ref input));

        Assert.Equal(9, ex.ColumnNumber);
        Assert.Equal("Unterminated string literal", ex.Message);
    }

    [Fact]
    public void ParseList_MissingCommaBetweenItems_ReportsSeparatorColumn()
    {
        var input = "[\"one\" \"two\"]";

        var ex = Assert.Throws<LineParsingException>(() => LineParser.ParseList(ref input));

        Assert.Equal(8, ex.ColumnNumber);
        Assert.Equal("Expected ',' between list items, found '\"'", ex.Message);
    }

    [Fact]
    public void ParseDictionary_MissingCommaBetweenEntries_ReportsSeparatorColumn()
    {
        var input = "{ (\"key1\", \"value1\") (\"key2\", \"value2\") }";

        var ex = Assert.Throws<LineParsingException>(() => LineParser.ParseDictionary(ref input));

        Assert.Equal(22, ex.ColumnNumber);
        Assert.Equal("Expected ',' between dictionary entries, found '('", ex.Message);
    }

    [Fact]
    public void ParseInt_InvalidToken_ReportsTokenStartColumn()
    {
        var input = "  abc 42";

        var ex = Assert.Throws<LineParsingException>(() => LineParser.ParseInt(ref input));

        Assert.Equal(3, ex.ColumnNumber);
        Assert.Equal("Invalid integer token 'abc'", ex.Message);
    }

    [Fact]
    public void ParseBool_InvalidToken_ReportsTokenStartColumn()
    {
        var input = "TrueValue";

        var ex = Assert.Throws<LineParsingException>(() => LineParser.ParseBool(ref input));

        Assert.Equal(1, ex.ColumnNumber);
        Assert.Equal("Invalid bool token 'TrueValue'", ex.Message);
    }

    [Fact]
    public void ParseByteArray_InvalidBase64_ReportsTokenStartColumn()
    {
        var input = "  abc remaining";

        var ex = Assert.Throws<LineParsingException>(() => LineParser.ParseByteArray(ref input));

        Assert.Equal(3, ex.ColumnNumber);
        Assert.Equal("Invalid base64 byte array", ex.Message);
    }
}
