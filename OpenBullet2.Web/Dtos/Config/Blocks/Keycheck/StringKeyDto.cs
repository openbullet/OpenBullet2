using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Conditions.Comparisons;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// A string key of the keychain.
/// </summary>
[PolyType("stringKey")]
[MapsFrom(typeof(StringKey))]
public class StringKeyDto : KeyDto
{
    /// <summary>
    /// The comparison condition.
    /// </summary>
    public StrComparison Comparison { get; set; }
}
