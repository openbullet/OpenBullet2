using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace OpenBullet2.Web.Dtos.Config.Convert;

/// <summary>
/// DTO used to convert a Stack of blocks to a LoliCode script.
/// </summary>
public class ConvertStackToLoliCodeDto
{
    /// <summary>
    /// The Stack of blocks to convert.
    /// </summary>
    [Required]
    public List<JsonElement> Stack { get; set; } = new();
}
