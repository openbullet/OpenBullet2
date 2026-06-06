using RuriLib.Models.Configs;
using Xunit;

namespace RuriLib.Tests.Models.Configs;

public class ConfigMetadataTests
{
    [Fact]
    public void Defaults_AreInitialized()
    {
        var metadata = new ConfigMetadata();

        Assert.NotNull(metadata.Plugins);
        Assert.Empty(metadata.Plugins);
        Assert.Equal(metadata.CreationDate, metadata.LastModified);
    }
}
