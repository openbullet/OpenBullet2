namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// DTO that contains a config's browser settings.
/// </summary>
public class ConfigBrowserSettingsDto
{
    /// <summary>
    /// The values of the status for which the browser should be closed
    /// when the bot ends its execution.
    /// </summary>
    public string[] QuitBrowserStatuses { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether to launch the browser in headless mode.
    /// </summary>
    public bool Headless { get; set; } = true;

    /// <summary>
    /// The extra command line arguments to pass to the browser's executable
    /// when opening a new browser.
    /// </summary>
    public string CommandLineArgs { get; set; } = "--disable-notifications";

    /// <summary>
    /// Whether to ignore HTTPS-related errors.
    /// </summary>
    public bool IgnoreHttpsErrors { get; set; } = false;

    /// <summary>
    /// Whether to only let the browser load documents and scripts,
    /// disregarding images and other heavy resources.
    /// </summary>
    public bool LoadOnlyDocumentAndScript { get; set; } = false;

    /// <summary>
    /// Whether to automatically dismiss confirmation dialogs that
    /// would otherwise take the focus away from the page.
    /// </summary>
    public bool DismissDialogs { get; set; } = false;

    /// <summary>
    /// The URLs that are blocked, so that the browser will not
    /// perform requests towards them.
    /// </summary>
    public List<string> BlockedUrls { get; set; } = new();
}
