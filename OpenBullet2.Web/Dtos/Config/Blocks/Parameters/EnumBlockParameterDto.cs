using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Parameters;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Parameters;

/// <summary>
/// An enum block parameter.
/// </summary>
[PolyType("enumParam")]
[MapsFrom(typeof(EnumParameter), false)]
public class EnumBlockParameterDto : BlockParameterDto
{
    /// <summary>
    /// The enum type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The default value.
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;

    /// <summary>
    /// The available values.
    /// </summary>
    public string[] Options { get; set; } = Array.Empty<string>();
}
