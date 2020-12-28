using System;
using System.Collections.Generic;

namespace OpenBullet2.Services
{
    public enum LogKind
    {
        Custom,
        Info,
        Success,
        Warning,
        Error
    }

    public struct JobLogEntry
    {
        public LogKind kind;
        public string message;
        public string color;
        public DateTime date;

        public JobLogEntry(LogKind kind, string message, string color)
        {
            this.kind = kind;
            this.message = message;
            this.color = color;
            date = DateTime.Now;
        }
    }

    public class JobLoggerService
    {
        private readonly PersistentSettingsService settings;
        private Dictionary<int, List<JobLogEntry>> logs = new Dictionary<int, List<JobLogEntry>>();
        public event EventHandler<int> NewLog; // The integer is the id of the job for which a new log came

        public JobLoggerService(PersistentSettingsService settings)
        {
            this.settings = settings;
        }

        public IEnumerable<JobLogEntry> GetLog(int jobId)
            => logs.ContainsKey(jobId) ? logs[jobId] : new List<JobLogEntry>();

        public void Log(int jobId, string message, LogKind kind = LogKind.Custom, string color = "white")
        {
            var entry = new JobLogEntry(kind, message, color);
            var maxBufferSize = settings.OpenBulletSettings.GeneralSettings.LogBufferSize;

            if (!logs.ContainsKey(jobId))
            {
                logs[jobId] = new List<JobLogEntry> { entry };
            }
            else
            {
                lock (logs[jobId])
                {
                    logs[jobId].Add(entry);

                    if (logs[jobId].Count > maxBufferSize && maxBufferSize > 0)
                        logs[jobId].RemoveRange(0, logs[jobId].Count - maxBufferSize);
                }
            }

            NewLog?.Invoke(this, jobId);
        }

        public void LogInfo(int jobId, string message) => Log(jobId, message, LogKind.Info, "white");
        public void LogSuccess(int jobId, string message) => Log(jobId, message, LogKind.Success, "greenyellow");
        public void LogWarning(int jobId, string message) => Log(jobId, message, LogKind.Warning, "orange");
        public void LogError(int jobId, string message) => Log(jobId, message, LogKind.Error, "tomato");

        public void Clear(int jobId)
        {
            if (!logs.ContainsKey(jobId))
                return;

            logs[jobId].Clear();
        }
    }
}