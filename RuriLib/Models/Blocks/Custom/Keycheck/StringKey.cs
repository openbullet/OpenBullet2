using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck;

/// <summary>
/// Keycheck entry for string comparisons.
/// </summary>
public class StringKey : Key
{
    /// <summary>
    /// Gets or sets the string comparison operator.
    /// </summary>
    public StrComparison Comparison { get; set; } = StrComparison.Contains;

    /// <summary>
    /// Initializes a new <see cref="StringKey"/>.
    /// </summary>
    public StringKey()
    {
        Left = BlockSettingFactory.CreateStringSetting(string.Empty, "data.SOURCE", SettingInputMode.Variable);
        Right = BlockSettingFactory.CreateStringSetting(string.Empty);
    }
}
