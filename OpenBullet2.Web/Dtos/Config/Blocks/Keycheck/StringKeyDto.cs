using RuriLib.Models.Conditions.Comparisons;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// A string key of the keychain.
/// </summary>
public class StringKeyDto : KeyDto
{
    /// <summary></summary>
    public StringKeyDto()
    {
        KeyType = KeyType.String;
    }

    /// <summary>
    /// The comparison condition.
    /// </summary>
    public StrComparison Comparison { get; set; }
}
