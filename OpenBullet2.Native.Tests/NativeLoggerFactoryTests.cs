using System.IO;
using Microsoft.Extensions.Configuration;
using OpenBullet2.Native;

namespace OpenBullet2.Native.Tests;

public sealed class NativeLoggerFactoryTests : IDisposable
{
    private readonly string tempDirectory = Path.Combine(Path.GetTempPath(), $"OB2_Native_Logs_{Guid.NewGuid():N}");

    [Fact]
    public void Create_MissingSerilogSinkName_UsesFallbackLogger()
    {
        Directory.CreateDirectory(tempDirectory);
        var logPath = Path.Combine(tempDirectory, "log-.txt");
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:WriteTo:0:Args:path"] = logPath
            })
            .Build();

        var logger = NativeLoggerFactory.Create(config, logPath);
        logger.Information("fallback logger test");
        (logger as IDisposable)?.Dispose();

        Assert.Contains(
            Directory.EnumerateFiles(tempDirectory, "log-*.txt"),
            file => new FileInfo(file).Length > 0);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, true);
        }
    }
}
