using RuriLib.Models.Configs;
using Xunit;

namespace RuriLib.Tests.Models.Configs;

public class ConfigTests
{
    [Fact]
    public void Defaults_AreSafe()
    {
        var config = new Config
        {
            Id = "test"
        };

        Assert.NotNull(config.Stack);
        Assert.Empty(config.Stack);
        Assert.Equal(string.Empty, config.LoliCodeScript);
        Assert.Equal(string.Empty, config.StartupLoliCodeScript);
        Assert.Equal(string.Empty, config.LoliScript);
        Assert.Equal(string.Empty, config.CSharpScript);
        Assert.Equal(string.Empty, config.StartupCSharpScript);
        Assert.NotNull(config.DeletedBlocksHistory);
        Assert.Empty(config.DeletedBlocksHistory);
    }

    [Fact]
    public void UpdateHashes_MakesNewConfigReportNoUnsavedChanges()
    {
        var config = new Config
        {
            Id = "test"
        };

        config.UpdateHashes();

        Assert.False(config.HasUnsavedChanges());
    }
}
