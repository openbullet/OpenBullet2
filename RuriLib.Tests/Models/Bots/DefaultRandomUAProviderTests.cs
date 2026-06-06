using RuriLib.Providers.UserAgents;
using RuriLib.Services;
using System;
using System.IO;
using Xunit;

namespace RuriLib.Tests.Models.Bots;

public class DefaultRandomUAProviderTests
{
    [Fact]
    public void Total_UsesConfiguredUserAgentsCount()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(DefaultRandomUAProviderTests)}-{Guid.NewGuid():N}");

        try
        {
            var settings = new RuriLibSettingsService(tempDirectory);
            settings.RuriLibSettings.GeneralSettings.UserAgents = ["ua-1", "ua-2", "ua-3"];

            var provider = new DefaultRandomUAProvider(settings);

            Assert.Equal(3, provider.Total);
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    [Fact]
    public void Generate_ReturnsConfiguredUserAgent()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(DefaultRandomUAProviderTests)}-{Guid.NewGuid():N}");

        try
        {
            var settings = new RuriLibSettingsService(tempDirectory);
            settings.RuriLibSettings.GeneralSettings.UserAgents = ["ua-1"];

            var provider = new DefaultRandomUAProvider(settings);

            Assert.Equal("ua-1", provider.Generate());
            Assert.Equal("ua-1", provider.Generate(UAPlatform.Mobile));
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }
}
