using OpenBullet2.Web.Dtos.Common;

namespace OpenBullet2.Web.Dtos.Hit;

/// <summary>
/// Filters to describe a subset of hits.
/// </summary>
public class HitFiltersDto : PaginationDto
{
    /// <summary>
    /// The search term to filter results by the hit data,
    /// captured data, config name, wordlist name or proxy. Optional.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// The hit type. Optional.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// The date and time of the oldest hit that should be retrieved.
    /// Optional.
    /// </summary>
    public DateTime? MinDate { get; set; } = null;

    /// <summary>
    /// The date and time of the newest hit that should be retrieved.
    /// Optional.
    /// </summary>
    public DateTime? MaxDate { get; set; } = null;
}
