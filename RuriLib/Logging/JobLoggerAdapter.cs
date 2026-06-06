using Microsoft.Extensions.Logging;
using System;

namespace RuriLib.Logging;

/// <summary>
/// Adapts an <see cref="IJobLogger"/> to the <see cref="ILogger{TCategoryName}"/> abstraction.
/// </summary>
internal class JobLoggerAdapter<TCategory>(IJobLogger logger, int jobId) : ILogger<TCategory>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;

    public bool IsEnabled(LogLevel logLevel)
        => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);

        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
            case LogLevel.Information:
                logger.LogInfo(jobId, message);
                break;

            case LogLevel.Warning:
                logger.LogWarning(jobId, message);
                break;

            case LogLevel.Error:
            case LogLevel.Critical:
                if (exception is not null)
                {
                    logger.LogException(jobId, exception);
                }
                else
                {
                    logger.LogError(jobId, message);
                }

                break;

            case LogLevel.None:
            default:
                break;
        }
    }
}
