using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Parameters;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Parameters;

/// <summary>
/// A boolean block parameter.
/// </summary>
[PolyType("boolParam")]
[MapsFrom(typeof(BoolParameter))]
public class BoolBlockParameterDto : BlockParameterDto
{
    /// <summary>
    /// The default value.
    /// </summary>
    public bool DefaultValue { get; set; } = false;
}
