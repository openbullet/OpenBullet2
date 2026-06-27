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

}
