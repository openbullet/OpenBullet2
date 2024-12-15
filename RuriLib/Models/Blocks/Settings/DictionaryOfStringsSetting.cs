using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// A setting that holds a dictionary of strings.
/// </summary>
public class DictionaryOfStringsSetting : Setting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public Dictionary<string, string> Value { get; set; } = [];
}
