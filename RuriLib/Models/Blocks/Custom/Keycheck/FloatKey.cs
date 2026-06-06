using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck;

/// <summary>
/// Keycheck entry for floating-point comparisons.
/// </summary>
public class FloatKey : Key
{
    /// <summary>
    /// Gets or sets the numeric comparison operator.
    /// </summary>
    public NumComparison Comparison { get; set; } = NumComparison.EqualTo;

    /// <summary>
    /// Initializes a new <see cref="FloatKey"/>.
    /// </summary>
    public FloatKey()
    {
        Left = BlockSettingFactory.CreateFloatSetting(string.Empty, mode: SettingInputMode.Variable);
        Right = BlockSettingFactory.CreateFloatSetting(string.Empty);
    }
}
