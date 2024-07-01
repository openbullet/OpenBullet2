namespace OpenBullet2.Web.Dtos.Common;

/// <summary>
/// DTO that contains pagination parameters.
/// </summary>
public class PaginationDto
{
    /// <summary>
    /// The page number.
    /// </summary>
    public int PageNumber { get; set; } = 0;

    /// <summary>
    /// The number of elements per page.
    /// </summary>
    public int PageSize { get; set; } = 25;
}
