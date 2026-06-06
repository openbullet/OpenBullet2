using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck;

/// <summary>
/// Keycheck entry for dictionary comparisons.
/// </summary>
public class DictionaryKey : Key
{
    /// <summary>
    /// Gets or sets the dictionary comparison operator.
    /// </summary>
    public DictComparison Comparison { get; set; } = DictComparison.HasKey;

    /// <summary>
    /// Initializes a new <see cref="DictionaryKey"/>.
    /// </summary>
    public DictionaryKey()
    {
        Left = BlockSettingFactory.CreateDictionaryOfStringsSetting(string.Empty, "data.HEADERS");
        Right = BlockSettingFactory.CreateStringSetting(string.Empty);
    }
}
