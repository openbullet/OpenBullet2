using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using System.Text.Json;
using Xunit;

namespace RuriLib.Tests.Models.Configs;

public class ConfigSettingsTests
{
    [Fact]
    public void Defaults_AreSafe()
    {
        var settings = new ConfigSettings();

        Assert.NotNull(settings.GeneralSettings);
        Assert.Equal(new[] { "SUCCESS", "NONE" }, settings.GeneralSettings.ContinueStatuses);
        Assert.NotNull(settings.ProxySettings);
        Assert.Equal(new[] { "BAN", "ERROR" }, settings.ProxySettings.BanProxyStatuses);
        Assert.NotNull(settings.ProxySettings.AllowedProxyTypes);
        Assert.NotNull(settings.InputSettings);
        Assert.Empty(settings.InputSettings.CustomInputs);
        Assert.NotNull(settings.DataSettings);
        Assert.Equal(new[] { "Default" }, settings.DataSettings.AllowedWordlistTypes);
        Assert.Empty(settings.DataSettings.DataRules);
        Assert.Empty(settings.DataSettings.Resources);
        Assert.NotNull(settings.BrowserSettings);
        Assert.Equal(BrowserAutomationEngine.Puppeteer, settings.BrowserSettings.Engine);
        Assert.Empty(settings.BrowserSettings.BlockedUrls);
        Assert.NotNull(settings.ScriptSettings);
    }

    [Fact]
    public void Deserialize_LegacyPuppeteerSettings_PopulatesBrowserSettings()
    {
        const string json = """
            {
              "PuppeteerSettings": {
                "Headless": false,
                "BlockedUrls": ["https://example.com"]
              }
            }
            """;

        var settings = JsonSerializer.Deserialize<ConfigSettings>(json);

        Assert.NotNull(settings);
        Assert.Equal(BrowserAutomationEngine.Puppeteer, settings.BrowserSettings.Engine);
        Assert.False(settings.BrowserSettings.Headless);
        Assert.Equal(new[] { "https://example.com" }, settings.BrowserSettings.BlockedUrls);
    }
}
