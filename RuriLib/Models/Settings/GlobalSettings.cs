namespace RuriLib.Models.Settings;

/// <summary>
/// Aggregates all persisted RuriLib settings sections.
/// </summary>
public class GlobalSettings
{
    /// <summary>Gets or sets the general settings.</summary>
    public GeneralSettings GeneralSettings { get; set; } = new();
    /// <summary>Gets or sets the captcha settings.</summary>
    public CaptchaSettings CaptchaSettings { get; set; } = new();
    /// <summary>Gets or sets the proxy settings.</summary>
    public ProxySettings ProxySettings { get; set; } = new();
    /// <summary>Gets or sets the Puppeteer settings.</summary>
    public PuppeteerSettings PuppeteerSettings { get; set; } = new();
    /// <summary>Gets or sets the Playwright settings.</summary>
    public PlaywrightSettings PlaywrightSettings { get; set; } = new();
    /// <summary>Gets or sets the Selenium settings.</summary>
    public SeleniumSettings SeleniumSettings { get; set; } = new();
}
