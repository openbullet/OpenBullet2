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

        public static void SaveCaptcha(byte[] bytes, BotData data)
        {
            var path = GetCaptchaPath(data);
            File.WriteAllBytes(path, bytes);
        }

        public static byte[] GetCaptcha(BotData data)
        {
            var path = GetCaptchaPath(data);
            return File.ReadAllBytes(path);
        }

        public static string GetScreenshotPath(BotData data)
        {
            Directory.CreateDirectory("Screenshots");
            return Path.Combine("Screenshots", $"{data.Line.Data.ToValidFileName()}.png");
        }

        public static string GetCaptchaPath(BotData data)
        {
            Directory.CreateDirectory("Captchas");
            return Path.Combine("Captchas", $"{data.Line.Data.ToValidFileName()}.png");
        }
    }
}
