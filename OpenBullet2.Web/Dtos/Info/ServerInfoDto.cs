namespace OpenBullet2.Web.Dtos.Info;

public class ServerInfoDto
{
    public TimeSpan LocalUtcOffset { get; set; }
    public DateTime StartTime { get; set; }
    public string OperatingSystem { get; set; } = string.Empty;
    public string CurrentWorkingDirectory { get; set; } = string.Empty;
    public string BuildNumber { get; set; } = string.Empty;
    public DateTime BuildDate { get; set; }
    public string ClientIpAddress { get; set; } = string.Empty;
}
