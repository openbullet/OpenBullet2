namespace OpenBullet2.Web.Interfaces;

public interface IUpdateService : IHostedService
{
    Version CurrentVersion { get; }
    Version RemoteVersion { get; }
    bool IsUpdateAvailable { get; }
    VersionType CurrentVersionType { get; }
}

public enum VersionType
{
    Alpha,
    Beta,
    Release
}
