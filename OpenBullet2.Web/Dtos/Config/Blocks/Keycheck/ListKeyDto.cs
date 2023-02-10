using RuriLib.Models.Conditions.Comparisons;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// A list key of the keychain.
/// </summary>
public class ListKeyDto : KeyDto
{
    /// <summary></summary>
    public ListKeyDto()
    {
        KeyType = KeyType.List;
    }

    /// <summary>
    /// The comparison condition.
    /// </summary>
    public ListComparison Comparison { get; set; }
}
