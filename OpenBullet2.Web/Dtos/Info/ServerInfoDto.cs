namespace OpenBullet2.Web.Dtos.Info;

/// <summary>
/// DTO that contains information about the server.
/// </summary>
public class ServerInfoDto
{
    /// <summary>
    /// The UTC offset of the server.
    /// </summary>
    public TimeSpan LocalUtcOffset { get; set; }

    /// <summary>
    /// The time when the server was started.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// The operating system where the server is hosted.
    /// </summary>
    public string OperatingSystem { get; set; } = string.Empty;

    /// <summary>
    /// The current working directory. All relative paths used
    /// in the software will be relative to this directory.
    /// </summary>
    public string CurrentWorkingDirectory { get; set; } = string.Empty;

    /// <summary>
    /// The build number of the current release.
    /// </summary>
    public string BuildNumber { get; set; } = string.Empty;

    /// <summary>
    /// The build date of the current release.
    /// </summary>
    public DateTime BuildDate { get; set; }

    /// <summary>
    /// The IP address of the calling client, for debugging purposes.
    /// </summary>
    public string ClientIpAddress { get; set; } = string.Empty;
}
