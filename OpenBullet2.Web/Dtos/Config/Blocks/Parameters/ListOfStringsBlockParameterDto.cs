using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Parameters;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Parameters;

/// <summary>
/// A list of strings block parameter.
/// </summary>
[PolyType("listOfStringsParam")]
[MapsFrom(typeof(ListOfStringsParameter))]
public class ListOfStringsBlockParameterDto : BlockParameterDto
{
    /// <summary>
    /// The default value.
    /// </summary>
    public List<string> DefaultValue { get; set; } = new();
}
