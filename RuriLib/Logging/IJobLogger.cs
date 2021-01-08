using System;

namespace RuriLib.Logging
{
    public interface IJobLogger
    {
        void Log(int jobId, string message, LogKind kind);
        void LogInfo(int jobId, string message);
        void LogWarning(int jobId, string message);
        void LogError(int jobId, string message);
        void LogException(int jobId, Exception ex);
    }
}
