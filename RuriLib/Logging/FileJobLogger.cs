using RuriLib.Functions.Files;
using RuriLib.Services;
using System;
using System.IO;

namespace RuriLib.Logging
{
    public class FileJobLogger : IJobLogger
    {
        private readonly RuriLibSettingsService settings;
        private readonly string baseFolder;

        public FileJobLogger(RuriLibSettingsService settings, string baseFolder)
        {
            this.settings = settings;
            this.baseFolder = baseFolder;
            Directory.CreateDirectory(baseFolder);
        }

        public void Log(int jobId, string message, LogKind kind = LogKind.Info)
        {
            if (settings.RuriLibSettings.GeneralSettings.LogJobActivityToFile)
            {
                var fileName = Path.Combine(baseFolder, $"job{jobId}.log");
                lock (FileLocker.GetHandle(fileName))
                {
                    var str = $"[{DateTime.Now.ToLongTimeString()}][{kind}] {message.Replace("\r\n", " ").Replace("\n", " ")}{Environment.NewLine}";
                    File.AppendAllText(fileName, str);
                }
            }
        }

        public void LogError(int jobId, string message)
            => Log(jobId, message, LogKind.Error);

        public void LogException(int jobId, Exception ex)
            => Log(jobId, $"({ex.GetType().Name}) {ex.Message}", LogKind.Error);

        public void LogInfo(int jobId, string message)
            => Log(jobId, message, LogKind.Info);

        public void LogWarning(int jobId, string message)
            => Log(jobId, message, LogKind.Warning);
    }
}
