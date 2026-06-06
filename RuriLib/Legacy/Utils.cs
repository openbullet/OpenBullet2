using RuriLib.Extensions;
using RuriLib.Models.Bots;
using System.IO;

namespace RuriLib.Legacy;

/// <summary>
/// Provides helpers for storing legacy screenshots and captcha images on disk.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Saves a screenshot for the provided bot data.
    /// </summary>
    /// <param name="bytes">The screenshot bytes.</param>
    /// <param name="data">The bot data used to build the output path.</param>
    public static void SaveScreenshot(byte[] bytes, BotData data)
    {
        var path = GetScreenshotPath(data);
        File.WriteAllBytes(path, bytes);
    }

    /// <summary>
    /// Reads the stored screenshot for the provided bot data.
    /// </summary>
    /// <param name="data">The bot data used to build the input path.</param>
    /// <returns>The screenshot bytes.</returns>
    public static byte[] GetScreenshot(BotData data)
    {
        var path = GetScreenshotPath(data);
        return File.ReadAllBytes(path);
    }

    /// <summary>
    /// Saves a captcha image for the provided bot data.
    /// </summary>
    /// <param name="bytes">The captcha image bytes.</param>
    /// <param name="data">The bot data used to build the output path.</param>
    public static void SaveCaptcha(byte[] bytes, BotData data)
    {
        var path = GetCaptchaPath(data);
        File.WriteAllBytes(path, bytes);
    }

    /// <summary>
    /// Reads the stored captcha image for the provided bot data.
    /// </summary>
    /// <param name="data">The bot data used to build the input path.</param>
    /// <returns>The captcha image bytes.</returns>
    public static byte[] GetCaptcha(BotData data)
    {
        var path = GetCaptchaPath(data);
        return File.ReadAllBytes(path);
    }

    /// <summary>
    /// Gets the legacy screenshot path for the provided bot data.
    /// </summary>
    /// <param name="data">The bot data used to build the path.</param>
    /// <returns>The full screenshot path.</returns>
    public static string GetScreenshotPath(BotData data)
    {
        Directory.CreateDirectory("Screenshots");
        return Path.Combine("Screenshots", $"{data.Line.Data.ToValidFileName()}.png");
    }

    /// <summary>
    /// Gets the legacy captcha image path for the provided bot data.
    /// </summary>
    /// <param name="data">The bot data used to build the path.</param>
    /// <returns>The full captcha image path.</returns>
    public static string GetCaptchaPath(BotData data)
    {
        Directory.CreateDirectory("Captchas");
        return Path.Combine("Captchas", $"{data.Line.Data.ToValidFileName()}.png");
    }
}
