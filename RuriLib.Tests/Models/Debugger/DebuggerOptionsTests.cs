using RuriLib.Models.Debugger;
using Xunit;

namespace RuriLib.Tests.Models.Debugger;

public class DebuggerOptionsTests
{
    [Fact]
    public void Defaults_AreSafe()
    {
        var options = new DebuggerOptions();

        Assert.Equal(string.Empty, options.WordlistType);
        Assert.Equal(string.Empty, options.TestData);
        Assert.Equal(string.Empty, options.TestProxy);
        Assert.NotNull(options.Variables);
        Assert.Empty(options.Variables);
    }
}
