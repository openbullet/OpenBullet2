using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck;

/// <summary>
/// Keycheck entry for boolean comparisons.
/// </summary>
public class BoolKey : Key
{
    /// <summary>
    /// Gets or sets the boolean comparison operator.
    /// </summary>
    public BoolComparison Comparison { get; set; } = BoolComparison.Is;

    /// <summary>
    /// Initializes a new <see cref="BoolKey"/>.
    /// </summary>
    public BoolKey()
    {
        Left = BlockSettingFactory.CreateBoolSetting(string.Empty, mode: SettingInputMode.Variable,
            defaultVariableName: "data.SOURCE");
        Right = BlockSettingFactory.CreateBoolSetting(string.Empty, true);
    }
}
