using OpenBullet2.Web.Dtos.Common;

namespace OpenBullet2.Web.Dtos.Hit;

/// <summary>
/// Filters to describe a subset of hits.
/// </summary>
public class PaginatedHitFiltersDto : PaginationDto
{
    /// <summary>
    /// The search term to filter results by the hit data,
    /// captured data, wordlist name or proxy. Optional.
    /// </summary>
    public string? SearchTerm { get; set; }
    
    /// <summary>
    /// The config name to filter results by. Optional.
    /// </summary>
    public string? ConfigName { get; set; }
    
    /// <summary>
    /// The hit types, comma separated. Optional.
    /// </summary>
    public string? Types { get; set; }
    
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
    
    /// <summary>
    /// The field to sort the hits by. Optional.
    /// </summary>
    public HitSortField? SortBy { get; set; } = null;
    
    /// <summary>
    /// Whether to sort the hits in descending order.
    /// Only used if <see cref="SortBy" /> is set.
    /// </summary>
    public bool SortDescending { get; set; } = true;
}
