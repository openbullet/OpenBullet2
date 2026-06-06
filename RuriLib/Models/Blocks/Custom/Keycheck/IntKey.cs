using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck;

/// <summary>
/// Keycheck entry for integer comparisons.
/// </summary>
public class IntKey : Key
{
    /// <summary>
    /// Gets or sets the numeric comparison operator.
    /// </summary>
    public NumComparison Comparison { get; set; } = NumComparison.EqualTo;

    /// <summary>
    /// Initializes a new <see cref="IntKey"/>.
    /// </summary>
    public IntKey()
    {
        Left = BlockSettingFactory.CreateIntSetting(string.Empty, mode: SettingInputMode.Variable,
            defaultVariableName: "data.RESPONSECODE");
        Right = BlockSettingFactory.CreateIntSetting(string.Empty);
    }
}
