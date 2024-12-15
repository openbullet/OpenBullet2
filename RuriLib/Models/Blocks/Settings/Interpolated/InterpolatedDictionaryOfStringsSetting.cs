using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings.Interpolated;

/// <summary>
/// A setting that holds an interpolated dictionary of strings.
/// </summary>
public class InterpolatedDictionaryOfStringsSetting : InterpolatedSetting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public Dictionary<string, string> Value { get; set; } = [];
}
