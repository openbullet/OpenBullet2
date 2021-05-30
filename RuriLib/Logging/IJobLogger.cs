using System;

namespace RuriLib.Logging
{
    /// <summary>
    /// Takes care of logging information produced by a job.
    /// </summary>
    public interface IJobLogger
    {
        /// <summary>
        /// Logs a generic <paramref name="message"/> to the log identified by the <paramref name="jobId"/>.
        /// </summary>
        void Log(int jobId, string message, LogKind kind);

        /// <summary>
        /// Logs an info <paramref name="message"/> to the log identified by the <paramref name="jobId"/>.
        /// </summary>
        void LogInfo(int jobId, string message);

        /// <summary>
        /// Logs a warning <paramref name="message"/> to the log identified by the <paramref name="jobId"/>.
        /// </summary>
        void LogWarning(int jobId, string message);

        /// <summary>
        /// Logs an error <paramref name="message"/> to the log identified by the <paramref name="jobId"/>.
        /// </summary>
        void LogError(int jobId, string message);

        /// <summary>
        /// Logs an <paramref name="exception"/> to the log identified by the <paramref name="jobId"/>.
        /// </summary>
        void LogException(int jobId, Exception exception);
    }
}
