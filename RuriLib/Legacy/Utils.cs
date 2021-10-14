using RuriLib.Extensions;
using RuriLib.Models.Bots;
using System.IO;

namespace RuriLib.Legacy
{
    public static class Utils
    {
        public static void SaveScreenshot(byte[] bytes, BotData data)
        {
            var path = GetScreenshotPath(data);
            File.WriteAllBytes(path, bytes);
        }

        public static byte[] GetScreenshot(BotData data)
        {
            var path = GetScreenshotPath(data);
            return File.ReadAllBytes(path);
        }

        private static string GetScreenshotPath(BotData data)
        {
            Directory.CreateDirectory("Screenshots");
            return Path.Combine("Screenshots", $"{data.Line.Data.ToValidFileName()}.png");
        }
    }
}
