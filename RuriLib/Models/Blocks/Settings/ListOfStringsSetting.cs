using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// A setting that holds a list of strings.
/// </summary>
public class ListOfStringsSetting : Setting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public List<string> Value { get; set; } = [];
}
