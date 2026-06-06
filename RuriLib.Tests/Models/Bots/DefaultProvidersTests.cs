using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Settings;
using RuriLib.Providers.Proxies;
using RuriLib.Providers.Browser;
using RuriLib.Providers.Playwright;
using RuriLib.Providers.Puppeteer;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.Security;
using RuriLib.Providers.Selenium;
using RuriLib.Services;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Xunit;
using RuntimeProxySettings = RuriLib.Models.Settings.ProxySettings;

namespace RuriLib.Tests.Models.Bots;

public class DefaultProvidersTests
{
    [Fact]
    public void DefaultRngProvider_GetNew_ReturnsDifferentInstances()
    {
        var provider = new DefaultRNGProvider();

        Assert.NotSame(provider.GetNew(), provider.GetNew());
    }

    [Fact]
    public void DefaultSecurityProvider_ReadsSettingsAndUsesNoCheckByDefault() => WithSettingsService(settings =>
                                                                                       {
                                                                                           settings.RuriLibSettings.GeneralSettings.RestrictBlocksToCWD = false;

                                                                                           var provider = new DefaultSecurityProvider(settings);

                                                                                           Assert.False(provider.RestrictBlocksToCWD);
                                                                                           Assert.Equal(X509RevocationMode.NoCheck, provider.X509RevocationMode);
                                                                                       });

    [Fact]
    public void DefaultPuppeteerBrowserProvider_ReadsChromePath() => WithSettingsService(settings =>
                                                                          {
                                                                              settings.RuriLibSettings.PuppeteerSettings.ChromeBinaryLocation = "chrome-path";

                                                                              var provider = new DefaultPuppeteerBrowserProvider(settings);

                                                                              Assert.Equal("chrome-path", provider.ChromeBinaryLocation);
                                                                          });

    [Fact]
    public void DefaultPlaywrightBrowserProvider_ReadsConfiguredValues() => WithSettingsService(settings =>
                                                                                {
                                                                                    settings.RuriLibSettings.PlaywrightSettings = new PlaywrightSettings
                                                                                    {
                                                                                        BrowserType = PlaywrightBrowserType.Firefox,
                                                                                        Source = PlaywrightBrowserSource.ExecutablePath,
                                                                                        ExecutablePath = "firefox-path"
                                                                                    };

                                                                                    var provider = new DefaultPlaywrightBrowserProvider(settings);

                                                                                    Assert.Equal(PlaywrightBrowserType.Firefox, provider.BrowserType);
                                                                                    Assert.Equal(PlaywrightBrowserSource.ExecutablePath, provider.Source);
                                                                                    Assert.Equal("firefox-path", provider.ExecutablePath);
                                                                                });

    [Fact]
    public void DefaultBrowserAutomationEngineResolver_ResolvesPuppeteer() => WithSettingsService(settings =>
                                                                                     {
                                                                                         var puppeteer = new DefaultPuppeteerBrowserProvider(settings);
                                                                                         var playwright = new DefaultPlaywrightBrowserProvider(settings);
                                                                                         var resolver = new DefaultBrowserAutomationEngineResolver(puppeteer, playwright);

                                                                                         Assert.IsType<PuppeteerBrowserAutomationEngine>(resolver.Resolve(BrowserAutomationEngine.Puppeteer));
                                                                                     });

    [Fact]
    public void DefaultBrowserAutomationEngineResolver_ResolvesPlaywright() => WithSettingsService(settings =>
                                                                                     {
                                                                                         var puppeteer = new DefaultPuppeteerBrowserProvider(settings);
                                                                                         var playwright = new DefaultPlaywrightBrowserProvider(settings);
                                                                                         var resolver = new DefaultBrowserAutomationEngineResolver(puppeteer, playwright);

                                                                                         Assert.IsType<PlaywrightBrowserAutomationEngine>(resolver.Resolve(BrowserAutomationEngine.Playwright));
                                                                                     });

    [Fact]
    public void DefaultSeleniumBrowserProvider_ReadsConfiguredValues() => WithSettingsService(settings =>
                                                                               {
                                                                                   settings.RuriLibSettings.SeleniumSettings = new SeleniumSettings
                                                                                   {
                                                                                       ChromeBinaryLocation = "chrome-path",
                                                                                       FirefoxBinaryLocation = "firefox-path",
                                                                                       BrowserType = SeleniumBrowserType.Firefox
                                                                                   };

                                                                                   var provider = new DefaultSeleniumBrowserProvider(settings);

                                                                                   Assert.Equal("chrome-path", provider.ChromeBinaryLocation);
                                                                                   Assert.Equal("firefox-path", provider.FirefoxBinaryLocation);
                                                                                   Assert.Equal(SeleniumBrowserType.Firefox, provider.BrowserType);
                                                                               });

    [Fact]
    public void DefaultGeneralSettingsProvider_ReadsConfiguredFlags() => WithSettingsService(settings =>
                                                                              {
                                                                                  settings.RuriLibSettings.GeneralSettings.VerboseMode = true;
                                                                                  settings.RuriLibSettings.GeneralSettings.LogAllResults = true;

                                                                                  var provider = new DefaultGeneralSettingsProvider(settings);

                                                                                  Assert.True(provider.VerboseMode);
                                                                                  Assert.True(provider.LogAllResults);
                                                                              });

    [Fact]
    public void DefaultProxySettingsProvider_ReadsTimeoutsAndMatchesKeys() => WithSettingsService(settings =>
                                                                                   {
                                                                                       settings.RuriLibSettings.ProxySettings = new RuntimeProxySettings
                                                                                       {
                                                                                           ProxyConnectTimeoutMilliseconds = 1234,
                                                                                           ProxyReadWriteTimeoutMilliseconds = 5678,
                                                                                           GlobalBanKeys = ["BANKEY"],
                                                                                           GlobalRetryKeys = ["RETRYKEY"]
                                                                                       };

                                                                                       var provider = new DefaultProxySettingsProvider(settings);

                                                                                       Assert.Equal(TimeSpan.FromMilliseconds(1234), provider.ConnectTimeout);
                                                                                       Assert.Equal(TimeSpan.FromMilliseconds(5678), provider.ReadWriteTimeout);
                                                                                       Assert.True(provider.ContainsBanKey("xxbankeyyy", out var banKey));
                                                                                       Assert.Equal("BANKEY", banKey);
                                                                                       Assert.False(provider.ContainsBanKey("xxbankeyyy", out _, caseSensitive: true));
                                                                                       Assert.True(provider.ContainsRetryKey("zzretrykeyzz", out var retryKey));
                                                                                       Assert.Equal("RETRYKEY", retryKey);
                                                                                       Assert.False(provider.ContainsRetryKey("", out var missingRetryKey));
                                                                                       Assert.Equal(string.Empty, missingRetryKey);
                                                                                   });

    private static void WithSettingsService(Action<RuriLibSettingsService> action)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(DefaultProvidersTests)}-{Guid.NewGuid():N}");

        try
        {
            action(new RuriLibSettingsService(tempDirectory));
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }
}
