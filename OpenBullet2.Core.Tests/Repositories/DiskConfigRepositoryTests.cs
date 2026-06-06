using OpenBullet2.Core.Repositories;
using RuriLib.Models.Configs;
using RuriLib.Services;

namespace OpenBullet2.Core.Tests.Repositories;

public sealed class DiskConfigRepositoryTests : IDisposable
{
    private readonly string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    [Fact]
    public async Task SaveAsync_LoliCodeConfig_DoesNotPersistBlankPluginEntries()
    {
        var settings = new RuriLibSettingsService(Path.Combine(tempDir, "settings"));
        var repository = new DiskConfigRepository(settings, Path.Combine(tempDir, "configs"));
        var config = new Config
        {
            Id = "test",
            Mode = ConfigMode.LoliCode,
            LoliCodeScript = """
                SET VAR myVar "first"
                BLOCK:ConstantString
                value = "second"
                => VAR @secondVar
                ENDBLOCK
                """
        };

        await repository.SaveAsync(config);

        Assert.Empty(config.Metadata.Plugins);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }
}
