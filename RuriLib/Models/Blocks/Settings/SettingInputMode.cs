namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// The supported input modes for a block setting.
/// </summary>
public enum SettingInputMode
{
    /// <summary>
    /// The setting reads its value from a variable.
    /// </summary>
    Variable,

    /// <summary>
    /// The setting stores a fixed value.
    /// </summary>
    Fixed,

    /// <summary>
    /// The setting stores an interpolated value.
    /// </summary>
    Interpolated
}
