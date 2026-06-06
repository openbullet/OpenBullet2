using RuriLib.Logging;
using Spectre.Console;
using System;

namespace OpenBullet2.Console;

internal class ConsoleJobLogger : IJobLogger
{
    public void Log(int jobId, string message, LogKind kind)
        => Write(kind, message);

    public void LogInfo(int jobId, string message)
        => Write(LogKind.Info, message);

    public void LogWarning(int jobId, string message)
        => Write(LogKind.Warning, message);

    public void LogError(int jobId, string message)
        => Write(LogKind.Error, message);

    public void LogException(int jobId, Exception exception)
        => Write(LogKind.Error, $"({exception.GetType().Name}) {exception.Message}");

    private static void Write(LogKind kind, string message)
    {
        var color = kind switch
        {
            LogKind.Warning => Color.Yellow,
            LogKind.Error => Color.Red,
            _ => Color.Grey
        };

        AnsiConsole.Write(new Text(message, new Style(foreground: color)));
        AnsiConsole.WriteLine();
    }
}
