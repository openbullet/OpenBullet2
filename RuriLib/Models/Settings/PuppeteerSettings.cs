namespace RuriLib.Models.Settings;

/// <summary>
/// Stores Puppeteer browser settings.
/// </summary>
public class PuppeteerSettings
{
    /// <summary>Gets or sets the Chrome binary location.</summary>
    public string ChromeBinaryLocation { get; set; } = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
    // public bool DrawMouseMovement { get; set; } = true;
}
