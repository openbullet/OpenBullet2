using System;
using RuriLib.Exceptions;
using Xunit;

namespace RuriLib.Tests.Exceptions;

public class LoliCodeParsingExceptionTests
{
    [Fact]
    public void Constructor_WithLineOnly_PreservesExistingMessageFormat()
    {
        var ex = new LoliCodeParsingException(12, "Could not parse the setting");

        Assert.Equal(12, ex.LineNumber);
        Assert.Null(ex.ColumnNumber);
        Assert.Equal("[Line 12] Could not parse the setting", ex.Message);
    }

    [Fact]
    public void Constructor_WithLineAndColumn_IncludesColumnInMessage()
    {
        var ex = new LoliCodeParsingException(12, 8, "Expected '='");

        Assert.Equal(12, ex.LineNumber);
        Assert.Equal(8, ex.ColumnNumber);
        Assert.Equal("[Line 12, Column 8] Expected '='", ex.Message);
    }

    [Fact]
    public void Constructor_WithLineColumnAndInner_PreservesInnerException()
    {
        var inner = new FormatException("inner");
        var ex = new LoliCodeParsingException(12, 8, "Expected '='", inner);

        Assert.Same(inner, ex.InnerException);
        Assert.Equal("[Line 12, Column 8] Expected '='", ex.Message);
    }
}
