namespace OpenBullet2.Web.Dtos.Config.Blocks;

/// <summary>
/// DTO that represents a lolicode block instance.
/// </summary>
public class LoliCodeBlockInstanceDto : BlockInstanceDto
{
    /// <summary>
    /// The lolicode script.
    /// </summary>
    public string Script { get; set; } = string.Empty;
}
