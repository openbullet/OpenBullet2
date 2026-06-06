using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// Represents a dictionary-of-strings block setting.
/// </summary>
public class DictionaryOfStringsSetting : Setting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public Dictionary<string, string> Value { get; set; } = [];
}
