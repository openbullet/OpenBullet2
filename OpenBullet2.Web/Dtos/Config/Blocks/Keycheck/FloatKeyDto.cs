using RuriLib.Models.Conditions.Comparisons;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// A floating point key of the keychain.
/// </summary>
public class FloatKeyDto : KeyDto
{
    /// <summary></summary>
    public FloatKeyDto()
    {
        KeyType = KeyType.Float;
    }

    /// <summary>
    /// The comparison condition.
    /// </summary>
    public NumComparison Comparison { get; set; }
}
