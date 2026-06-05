using System;
using RuriLib.Exceptions;
using Xunit;

namespace RuriLib.Tests.Exceptions;

public class LineParsingExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_HasNoColumn()
    {
        var ex = new LineParsingException("Expected token");

        Assert.Null(ex.ColumnNumber);
        Assert.Equal("Expected token", ex.Message);
    }

    [Fact]
    public void Constructor_WithColumn_StoresColumn()
    {
        var ex = new LineParsingException(4, "Expected token");

        Assert.Equal(4, ex.ColumnNumber);
        Assert.Equal("Expected token", ex.Message);
    }

    [Fact]
    public void Constructor_WithColumnAndInner_PreservesInnerException()
    {
        var inner = new FormatException("inner");
        var ex = new LineParsingException(4, "Expected token", inner);

        Assert.Equal(4, ex.ColumnNumber);
        Assert.Same(inner, ex.InnerException);
    }
}
