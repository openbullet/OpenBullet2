using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Settings.Configuration;
using Serilog.Sinks.File;
using System;
using System.IO;

namespace OpenBullet2.Native;

internal static class NativeLoggerFactory
{
    public static ILogger Create(IConfiguration config, string fallbackLogPath)
    {
        try
        {
            return CreateConfiguredLogger(config);
        }
        catch (Exception ex)
        {
            var logger = CreateFallbackLogger(fallbackLogPath);
            logger.Warning(ex, "Failed to configure Serilog from appsettings.json, using fallback logger");
            return logger;
        }
    }

    private static ILogger CreateConfiguredLogger(IConfiguration config) =>
        new LoggerConfiguration()
            .ReadFrom.Configuration(
                config,
                new ConfigurationReaderOptions(
                    typeof(FileLoggerConfigurationExtensions).Assembly,
                    typeof(LoggerEnrichmentConfigurationExtensions).Assembly,
                    typeof(CompactJsonFormatter).Assembly))
            .CreateLogger();

    private static ILogger CreateFallbackLogger(string logPath)
    {
        var logDirectory = Path.GetDirectoryName(logPath);

        if (!string.IsNullOrWhiteSpace(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.File(
                new CompactJsonFormatter(),
                logPath,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 1_000_000)
            .CreateLogger();
    }
}
