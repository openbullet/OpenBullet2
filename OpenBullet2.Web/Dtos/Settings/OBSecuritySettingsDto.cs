namespace OpenBullet2.Web.Dtos.Settings;

/// <summary>
/// Settings related to security.
/// </summary>
public class OBSecuritySettingsDto
{
    /// <summary>
    /// Whether to allow OpenBullet2 (mainly blocks and file system viewer) to access
    /// the whole system or only the UserData folder and its subfolders.
    /// </summary>
    public bool AllowSystemWideFileAccess { get; set; } = false;

    /// <summary>
    /// Whether to require admin login when accessing the UI. Use this when exposing
    /// an OpenBullet 2 instance on the unprotected internet.
    /// </summary>
    public bool RequireAdminLogin { get; set; } = false;

    /// <summary>
    /// The username for the admin user.
    /// </summary>
    public string AdminUsername { get; set; } = "admin";
    
    /// <summary>
    /// The API key that the admin can use to authenticate to the API.
    /// If empty, the admin will not be able to use the API.
    /// </summary>
    public string AdminApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The number of hours that the admin session will last before requiring another login.
    /// </summary>
    public int AdminSessionLifetimeHours { get; set; } = 24;

    /// <summary>
    /// The number of hours that the guest session will last before requiring another login.
    /// </summary>
    public int GuestSessionLifetimeHours { get; set; } = 24;

    /// <summary>
    /// Whether to use HTTPS redirection when the application is accessed via HTTP.
    /// </summary>
    public bool HttpsRedirect { get; set; } = false;
}
