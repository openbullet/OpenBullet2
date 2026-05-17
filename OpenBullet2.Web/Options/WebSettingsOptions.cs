namespace OpenBullet2.Web.Options;

/// <summary>
/// Typed configuration bound from the web application's Settings section.
/// </summary>
public class WebSettingsOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Settings";

    /// <summary>
    /// The default root folder used for user data.
    /// </summary>
    public const string DefaultUserDataFolder = "UserData";

    /// <summary>
    /// The default allowed origin for the web client.
    /// </summary>
    public const string DefaultAllowedOrigin = "http://localhost:4200";

    /// <summary>
    /// The root folder used for user data.
    /// </summary>
    public string UserDataFolder { get; set; } = DefaultUserDataFolder;

    /// <summary>
    /// The allowed origin for CORS requests from the web client.
    /// </summary>
    public string AllowedOrigin { get; set; } = DefaultAllowedOrigin;
}
