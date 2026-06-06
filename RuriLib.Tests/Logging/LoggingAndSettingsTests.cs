using RuriLib.Logging;
using RuriLib.Models.Settings;
using RuriLib.Services;
using System;
using System.IO;
using Xunit;

namespace RuriLib.Tests.Logging;

public class LoggingAndSettingsTests
{
    [Fact]
    public void BotLoggerEntry_Defaults_AreSafe()
    {
        var entry = new BotLoggerEntry();

        Assert.Equal(string.Empty, entry.Message);
        Assert.Equal(string.Empty, entry.Color);
        Assert.False(entry.CanViewAsHtml);
    }

    [Fact]
    public void GlobalSettings_Defaults_InitializeNestedSettings()
    {
        var settings = new GlobalSettings();

        Assert.NotNull(settings.GeneralSettings);
        Assert.NotNull(settings.CaptchaSettings);
        Assert.NotNull(settings.ProxySettings);
        Assert.NotNull(settings.PuppeteerSettings);
        Assert.NotNull(settings.PlaywrightSettings);
        Assert.NotNull(settings.SeleniumSettings);
    }

    [Fact]
    public void GeneralAndProxySettings_DefaultCollections_AreInitialized()
    {
        var general = new GeneralSettings();
        var proxy = new ProxySettings();

        Assert.Empty(general.UserAgents);
        Assert.Empty(proxy.GlobalBanKeys);
        Assert.Empty(proxy.GlobalRetryKeys);
    }

    [Fact]
    public void BotLogger_WhenDisabled_DoesNotStoreEntries()
    {
        var logger = new BotLogger
        {
            Enabled = false
        };

        logger.Log("test");

        Assert.Empty(logger.Entries);
    }

    [Fact]
    public void FileJobLogger_WritesSingleLineEntry()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            var settingsService = new RuriLibSettingsService(baseFolder);
            settingsService.RuriLibSettings.GeneralSettings.LogJobActivityToFile = true;
            var logger = new FileJobLogger(settingsService, baseFolder);

            logger.LogInfo(7, $"hello{Environment.NewLine}world");

            var contents = File.ReadAllText(Path.Combine(baseFolder, "job7.log"));
            Assert.Contains("[Info] hello world", contents);
        }
        finally
        {
            if (Directory.Exists(baseFolder))
            {
                Directory.Delete(baseFolder, true);
            }
        }
    }

    [Fact]
    public void FileJobLogger_WhenDisabled_DoesNotCreateLogFile()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            var settingsService = new RuriLibSettingsService(baseFolder);
            settingsService.RuriLibSettings.GeneralSettings.LogJobActivityToFile = false;
            var logger = new FileJobLogger(settingsService, baseFolder);

            logger.LogInfo(7, "ignored");

            Assert.False(File.Exists(Path.Combine(baseFolder, "job7.log")));
        }
        finally
        {
            if (Directory.Exists(baseFolder))
            {
                Directory.Delete(baseFolder, true);
            }
        }
    }

    [Fact]
    public void FileJobLogger_LogException_WritesExceptionTypeAndMessage()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            var settingsService = new RuriLibSettingsService(baseFolder);
            settingsService.RuriLibSettings.GeneralSettings.LogJobActivityToFile = true;
            var logger = new FileJobLogger(settingsService, baseFolder);

            logger.LogException(7, new InvalidOperationException("broken"));

            var contents = File.ReadAllText(Path.Combine(baseFolder, "job7.log"));
            Assert.Contains("[Error] (InvalidOperationException) broken", contents);
        }
        finally
        {
            if (Directory.Exists(baseFolder))
            {
                Directory.Delete(baseFolder, true);
            }
        }
    }
}
