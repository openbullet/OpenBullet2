namespace RuriLib.Models.Settings;

/// <summary>
/// Stores Selenium browser settings.
/// </summary>
public class SeleniumSettings
{
    /// <summary>Gets or sets the browser type.</summary>
    public SeleniumBrowserType BrowserType { get; set; } = SeleniumBrowserType.Chrome;
    /// <summary>Gets or sets the Chrome binary location.</summary>
    public string ChromeBinaryLocation { get; set; } = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
    /// <summary>Gets or sets the Firefox binary location.</summary>
    public string FirefoxBinaryLocation { get; set; } = @"C:\Program Files\Mozilla Firefox\firefox.exe";
    // public bool DrawMouseMovement { get; set; } = true;
}

/// <summary>
/// Identifies the Selenium browser to launch.
/// </summary>
public enum SeleniumBrowserType
{
    /// <summary>Google Chrome.</summary>
    Chrome,
    /// <summary>Mozilla Firefox.</summary>
    Firefox
}
