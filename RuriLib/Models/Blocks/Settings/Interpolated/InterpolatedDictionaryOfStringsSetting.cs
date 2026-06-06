using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings.Interpolated;

/// <summary>
/// Represents an interpolated dictionary-of-strings block setting.
/// </summary>
public class InterpolatedDictionaryOfStringsSetting : InterpolatedSetting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public Dictionary<string, string> Value { get; set; } = [];
}
