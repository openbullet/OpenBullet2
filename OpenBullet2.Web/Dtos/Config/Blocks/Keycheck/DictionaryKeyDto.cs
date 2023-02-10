using RuriLib.Models.Conditions.Comparisons;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// A dictionary key of the keychain.
/// </summary>
public class DictionaryKeyDto : KeyDto
{
    /// <summary></summary>
    public DictionaryKeyDto()
    {
        KeyType = KeyType.Dictionary;
    }

    /// <summary>
    /// The comparison condition.
    /// </summary>
    public DictComparison Comparison { get; set; }
}
