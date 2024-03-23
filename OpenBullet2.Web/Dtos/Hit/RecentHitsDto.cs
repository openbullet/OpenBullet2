namespace OpenBullet2.Web.Dtos.Hit;

/// <summary>
/// DTO that contains information about recent hits for the chart.
/// </summary>
public class RecentHitsDto
{
    /// <summary>
    /// The dates on the x axis.
    /// </summary>
    public IEnumerable<DateTime> Dates { get; set; } = Array.Empty<DateTime>();

    /// <summary>
    /// The time-series for each config.
    /// </summary>
    public Dictionary<string, List<int>> Hits { get; set; } = new();
}
