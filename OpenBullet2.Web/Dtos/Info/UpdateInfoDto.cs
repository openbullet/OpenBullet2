using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Dtos.Info;

/// <summary>
/// DTO that contains information about new updates.
/// </summary>
public class UpdateInfoDto
{
    /// <summary>
    /// The current version of the software.
    /// </summary>
    public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>
    /// The latest remotely available version of the software.
    /// </summary>
    public string RemoteVersion { get; set; } = string.Empty;

    /// <summary>
    /// Whether an update is available.
    /// </summary>
    public bool IsUpdateAvailable { get; set; }

    /// <summary>
    /// The type of the current version.
    /// </summary>
    public VersionType CurrentVersionType { get; set; }

    /// <summary>
    /// The type of the remote version.
    /// </summary>
    public VersionType RemoteVersionType { get; set; }
}
