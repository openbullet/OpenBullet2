using OpenBullet2.Core.Services;
using OpenBullet2.Logging;
using RuriLib.Logging;

namespace OpenBullet2.Web.Tests.Unit.Core;

public sealed class MemoryJobLoggerTests : IDisposable
{
    private readonly string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    [Fact]
    public void Log_WhenDisabled_DoesNotStoreEntriesOrRaiseEvent()
    {
        var service = CreateSettingsService();
        service.Settings.GeneralSettings.EnableJobLogging = false;
        var logger = new MemoryJobLogger(service);
        var events = 0;
        logger.NewLog += (_, _) => events++;

        logger.LogInfo(1, "ignored");

        Assert.Empty(logger.GetLog(1));
        Assert.Equal(0, events);
    }

    [Fact]
    public void Log_RespectsBufferSizeAndRaisesEvent()
    {
        var service = CreateSettingsService();
        service.Settings.GeneralSettings.EnableJobLogging = true;
        service.Settings.GeneralSettings.LogBufferSize = 2;
        var logger = new MemoryJobLogger(service);
        var events = new List<int>();
        logger.NewLog += (_, jobId) => events.Add(jobId);

        logger.LogInfo(7, "first");
        logger.LogWarning(7, "second");
        logger.LogError(7, "third");

        var log = logger.GetLog(7).ToArray();
        Assert.Equal([7, 7, 7], events);
        Assert.Equal(2, log.Length);
        Assert.Equal(LogKind.Warning, log[0].kind);
        Assert.Equal("second", log[0].message);
        Assert.Equal(LogKind.Error, log[1].kind);
        Assert.Equal("third", log[1].message);
    }

    [Fact]
    public void Clear_RemovesOnlySelectedJobLog()
    {
        var service = CreateSettingsService();
        service.Settings.GeneralSettings.EnableJobLogging = true;
        var logger = new MemoryJobLogger(service);
        logger.LogInfo(1, "first");
        logger.LogInfo(2, "second");

        logger.Clear(1);

        Assert.Empty(logger.GetLog(1));
        Assert.Single(logger.GetLog(2));
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    private OpenBulletSettingsService CreateSettingsService()
        => new(tempDir);
}
