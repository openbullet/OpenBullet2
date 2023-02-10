using RuriLib.Models.Conditions.Comparisons;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// A boolean key of the keychain.
/// </summary>
public class BoolKeyDto : KeyDto
{
    /// <summary></summary>
    public BoolKeyDto()
    {
        KeyType = KeyType.Bool;
    }

    /// <summary>
    /// The comparison condition.   
    /// </summary>
    public BoolComparison Comparison { get; set; }
}
