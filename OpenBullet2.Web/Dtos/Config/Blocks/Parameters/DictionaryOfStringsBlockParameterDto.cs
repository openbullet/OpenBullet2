using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Parameters;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Parameters;

/// <summary>
/// A dictionary of strings block parameter.
/// </summary>
[PolyType("dictionaryOfStringsParam")]
[MapsFrom(typeof(DictionaryOfStringsParameter))]
public class DictionaryOfStringsBlockParameterDto : BlockParameterDto
{
    /// <summary>
    /// The default value.
    /// </summary>
    public Dictionary<string, string> DefaultValue { get; set; } = new();
}
