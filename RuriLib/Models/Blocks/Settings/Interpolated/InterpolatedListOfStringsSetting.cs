using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings.Interpolated;

/// <summary>
/// Represents an interpolated list-of-strings block setting.
/// </summary>
public class InterpolatedListOfStringsSetting : InterpolatedSetting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public List<string> Value { get; set; } = [];
}
