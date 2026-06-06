using CaptchaSharp.Enums;

namespace RuriLib.Models.Captchas;

/// <summary>
/// Represents a created captcha challenge and its identifier.
/// </summary>
public class CaptchaInfo
{
    /// <summary>
    /// Gets or sets the captcha identifier returned by the provider.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the captcha type.
    /// </summary>
    public CaptchaType Type { get; set; }
}
