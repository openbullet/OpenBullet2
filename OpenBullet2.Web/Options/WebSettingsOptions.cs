namespace OpenBullet2.Web.Options;

/// <summary>
/// Typed configuration bound from the web application's Settings section.
/// </summary>
internal sealed class WebSettingsOptions
{
    /// <summary>
    /// The default allowed origin for the web client.
    /// </summary>
    public const string DefaultAllowedOrigin = "http://localhost:4200";

    /// <summary>
    /// The allowed origin for CORS requests from the web client.
    /// </summary>
    public string AllowedOrigin { get; set; } = DefaultAllowedOrigin;
}
