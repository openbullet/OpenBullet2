using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Custom.Keycheck;

/// <summary>
/// Base class for a keycheck condition entry.
/// </summary>
public class Key
{
    /// <summary>
    /// Gets or sets the left operand.
    /// </summary>
    public BlockSetting Left { get; set; } = BlockSettingFactory.CreateStringSetting(string.Empty);
    /// <summary>
    /// Gets or sets the right operand.
    /// </summary>
    public BlockSetting Right { get; set; } = BlockSettingFactory.CreateStringSetting(string.Empty);
}
