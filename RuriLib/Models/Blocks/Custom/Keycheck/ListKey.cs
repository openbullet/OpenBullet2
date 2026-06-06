using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck;

/// <summary>
/// Keycheck entry for list comparisons.
/// </summary>
public class ListKey : Key
{
    /// <summary>
    /// Gets or sets the list comparison operator.
    /// </summary>
    public ListComparison Comparison { get; set; } = ListComparison.Contains;

    /// <summary>
    /// Initializes a new <see cref="ListKey"/>.
    /// </summary>
    public ListKey()
    {
        Left = BlockSettingFactory.CreateListOfStringsSetting(string.Empty, "data.SOURCE");
        Right = BlockSettingFactory.CreateStringSetting(string.Empty);
    }
}
