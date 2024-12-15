using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings.Interpolated;

/// <summary>
/// A setting that holds an interpolated list of strings.
/// </summary>
public class InterpolatedListOfStringsSetting : InterpolatedSetting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public List<string> Value { get; set; } = [];
}
