using System;
using RuriLib.Extensions;
using Xunit;

namespace RuriLib.Tests.Extensions;

public class ExceptionExtensionsTests
{
    [Fact]
    public void PrettyPrint_IncludesAllInnerExceptions()
    {
        var ex = new InvalidOperationException("outer",
            new ArgumentException("middle",
                new Exception("inner")));

        var result = ex.PrettyPrint();

        Assert.Equal(
            "System.InvalidOperationException: outer | System.ArgumentException: middle | System.Exception: inner",
            result);
    }
}
