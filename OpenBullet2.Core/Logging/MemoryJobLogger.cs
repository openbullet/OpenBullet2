using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Services;
using RuriLib.Logging;
using System;
using System.Collections.Generic;

namespace OpenBullet2.Logging;

public struct JobLogEntry(LogKind kind, string message, string color)
{
    public LogKind kind = kind;
    public string message = message;
    public string color = color;
    public DateTime date = DateTime.Now;
}

/// <summary>
/// An in-memory logger for job operations.
/// </summary>
public class MemoryJobLogger(OpenBulletSettingsService settingsService)
{
    private readonly Dictionary<int, List<JobLogEntry>> logs = [];
    private readonly object locker = new();
    private readonly OpenBulletSettings settings = settingsService.Settings;
    public event EventHandler<int>? NewLog; // The integer is the id of the job for which a new log came

    public IEnumerable<JobLogEntry> GetLog(int jobId)
    {
        lock (locker)
        {
            // Return a copy so we can keep modifying the original one without worrying about thread safety
            return logs.ContainsKey(jobId) ? logs[jobId].ToArray() : new List<JobLogEntry>();
        }
    }

    public void Log(int jobId, string message, LogKind kind = LogKind.Custom, string color = "white")
    {
        if (!settings.GeneralSettings.EnableJobLogging)
        {
            return;
        }

        var entry = new JobLogEntry(kind, message, color);
        var maxBufferSize = settings.GeneralSettings.LogBufferSize;

        lock (locker)
        {
            if (!logs.ContainsKey(jobId))
            {
                logs[jobId] = [entry];
            }
            else
            {
                lock (logs[jobId])
                {
                    logs[jobId].Add(entry);

                    if (logs[jobId].Count > maxBufferSize && maxBufferSize > 0)
                    {
                        logs[jobId].RemoveRange(0, logs[jobId].Count - maxBufferSize);
                    }
                }
            }
        }

        NewLog?.Invoke(this, jobId);
    }

    public void LogInfo(int jobId, string message) => Log(jobId, message, LogKind.Info, "var(--fg-primary)");
    public void LogSuccess(int jobId, string message) => Log(jobId, message, LogKind.Success, "var(--fg-hit)");
    public void LogWarning(int jobId, string message) => Log(jobId, message, LogKind.Warning, "var(--fg-custom)");
    public void LogError(int jobId, string message) => Log(jobId, message, LogKind.Error, "var(--fg-fail)");

    public void Clear(int jobId)
    {
        if (!logs.ContainsKey(jobId))
        {
            return;
        }

        logs[jobId].Clear();
    }
}
