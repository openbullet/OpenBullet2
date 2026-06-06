using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// Represents a list-of-strings block setting.
/// </summary>
public class ListOfStringsSetting : Setting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public List<string> Value { get; set; } = [];
}
