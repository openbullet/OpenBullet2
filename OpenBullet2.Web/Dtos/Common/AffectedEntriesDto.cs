namespace OpenBullet2.Web.Dtos.Common;

/// <summary>
/// DTO that contains information about entries affected
/// by a certain action.
/// </summary>
public class AffectedEntriesDto
{
    /// <summary>
    /// How many entries were affected.
    /// </summary>
    public long Count { get; set; }
}
