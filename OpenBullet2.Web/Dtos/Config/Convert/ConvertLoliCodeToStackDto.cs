namespace OpenBullet2.Web.Dtos.Config.Convert;

/// <summary>
/// DTO used to convert a LoliCode script to a Stack of blocks.
/// </summary>
public class ConvertLoliCodeToStackDto
{
    /// <summary>
    /// The LoliCode script to convert.
    /// </summary>
    public string LoliCode { get; set; } = string.Empty;
}
