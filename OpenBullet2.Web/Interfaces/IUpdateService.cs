namespace OpenBullet2.Web.Interfaces;

/// <summary>
/// A service that can track versions and monitor new updates.
/// </summary>
public interface IUpdateService : IHostedService
{
    /// <summary>
    /// The current version of the software.
    /// </summary>
    Version CurrentVersion { get; }

    /// <summary>
    /// The latest remotely available version of the software.
    /// </summary>
    Version RemoteVersion { get; }

    /// <summary>
    /// Whether an update is available.
    /// </summary>
    bool IsUpdateAvailable { get; }

    /// <summary>
    /// The type of the current version.
    /// </summary>
    VersionType CurrentVersionType { get; }

    /// <summary>
    /// The type of the remote version.
    /// </summary>
    VersionType RemoteVersionType { get; }
}

/// <summary>
/// A version type.
/// </summary>
public enum VersionType
{
    /// <summary>
    /// Alpha version (0.0.x).
    /// </summary>
    Alpha,

    /// <summary>
    /// Beta version (0.x.y).
    /// </summary>
    Beta,

    /// <summary>
    /// Release version (x.y.z).
    /// </summary>
    Release
}
