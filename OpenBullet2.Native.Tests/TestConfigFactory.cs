using RuriLib.Models.Configs;
using RuriLib.Services;

namespace OpenBullet2.Native.Tests;

internal static class TestConfigFactory
{
    public static Config Create(RuriLibSettingsService rlSettingsService, ConfigMode mode = ConfigMode.Stack)
    {
        var config = new Config
        {
            Id = Guid.NewGuid().ToString("N"),
            Mode = mode,
            Readme = "# Test Config"
        };

        config.Metadata.Name = $"Test {mode}";
        config.Metadata.Author = "Native Tests";
        config.Metadata.Category = "Smoke";

        var firstWordlistType = rlSettingsService.Environment.WordlistTypes.FirstOrDefault()?.Name;

        if (!string.IsNullOrWhiteSpace(firstWordlistType))
        {
            config.Settings.DataSettings.AllowedWordlistTypes = [firstWordlistType];
        }

        return config;
    }
}
