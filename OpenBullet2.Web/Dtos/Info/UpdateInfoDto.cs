using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Dtos.Info;

public class UpdateInfoDto
{
    public string CurrentVersion { get; set; } = string.Empty;
    public string RemoteVersion { get; set; } = string.Empty;
    public bool IsUpdateAvailable { get; set; }
    public VersionType CurrentVersionType { get; set; }
}
