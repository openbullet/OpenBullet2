using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Conditions.Comparisons;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// A boolean key of the keychain.
/// </summary>
[PolyType("boolKey")]
[MapsFrom(typeof(BoolKey))]
public class BoolKeyDto : KeyDto
{
    /// <summary>
    /// The comparison condition.
    /// </summary>
    public BoolComparison Comparison { get; set; }
}
