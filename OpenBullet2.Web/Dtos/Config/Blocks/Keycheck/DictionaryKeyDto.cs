using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Conditions.Comparisons;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// A dictionary key of the keychain.
/// </summary>
[PolyType("dictionaryKey")]
[MapsFrom(typeof(DictionaryKey))]
public class DictionaryKeyDto : KeyDto
{
    /// <summary>
    /// The comparison condition.
    /// </summary>
    public DictComparison Comparison { get; set; }
}
