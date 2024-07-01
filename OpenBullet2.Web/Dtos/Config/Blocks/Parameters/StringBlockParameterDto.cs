using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Parameters;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Parameters;

/// <summary>
/// A string block parameter.
/// </summary>
[PolyType("stringParam")]
[MapsFrom(typeof(StringParameter))]
public class StringBlockParameterDto : BlockParameterDto
{
    /// <summary>
    /// The default value.
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;

    /// <summary>
    /// Whether the parameter accepts line breaks.
    /// </summary>
    public bool MultiLine { get; set; }
}
