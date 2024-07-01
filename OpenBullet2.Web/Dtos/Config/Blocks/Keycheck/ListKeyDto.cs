using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Conditions.Comparisons;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// A list key of the keychain.
/// </summary>
[PolyType("listKey")]
[MapsFrom(typeof(ListKey))]
public class ListKeyDto : KeyDto
{
    /// <summary>
    /// The comparison condition.
    /// </summary>
    public ListComparison Comparison { get; set; }
}
