using RuriLib.Models.Conditions.Comparisons;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// An integer key of the keychain.
/// </summary>
public class IntKeyDto : KeyDto
{
    /// <summary></summary>
    public IntKeyDto()
    {
        KeyType = KeyType.Int;
    }

    /// <summary>
    /// The comparison condition.
    /// </summary>
    public NumComparison Comparison { get; set; }
}
