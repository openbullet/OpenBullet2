using RuriLib.Functions.Files;
using RuriLib.Services;
using System;
using System.IO;

namespace RuriLib.Logging
{
    /// <summary>
    /// Takes care of logging information produced by a job to file.
    /// </summary>
    public class FileJobLogger : IJobLogger
    {
        private readonly RuriLibSettingsService settings;
        private readonly string baseFolder;

        /// <summary>
        /// Creates a <see cref="FileJobLogger"/> that will log information to a
        /// file created in the given <paramref name="baseFolder"/>.
        /// </summary>
        public FileJobLogger(RuriLibSettingsService settings, string baseFolder)
        {
            this.settings = settings;
            this.baseFolder = baseFolder;
            Directory.CreateDirectory(baseFolder);
        }

        /// <inheritdoc/>
        public void Log(int jobId, string message, LogKind kind = LogKind.Info)
        {
            if (!settings.RuriLibSettings.GeneralSettings.LogJobActivityToFile) return;
            
            var fileName = Path.Combine(baseFolder, $"job{jobId}.log");
            lock (FileLocker.GetHandle(fileName))
            {
                var str = $"[{DateTime.Now.ToLongTimeString()}][{kind}] {message.Replace("\r\n", " ").Replace("\n", " ")}{Environment.NewLine}";
                File.AppendAllText(fileName, str);
            }
        }

        /// <inheritdoc/>
        public void LogError(int jobId, string message)
            => Log(jobId, message, LogKind.Error);

        /// <inheritdoc/>
        public void LogException(int jobId, Exception exception)
            => Log(jobId, $"({exception.GetType().Name}) {exception.Message}", LogKind.Error);

        /// <inheritdoc/>
        public void LogInfo(int jobId, string message)
            => Log(jobId, message, LogKind.Info);

        /// <inheritdoc/>
        public void LogWarning(int jobId, string message)
            => Log(jobId, message, LogKind.Warning);
    }
}
