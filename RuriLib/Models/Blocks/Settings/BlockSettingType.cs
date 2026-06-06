namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// The available fixed setting value types.
/// </summary>
public enum BlockSettingType
{
    /// <summary>
    /// No setting type.
    /// </summary>
    None,

    /// <summary>
    /// A string setting.
    /// </summary>
    String,

    /// <summary>
    /// An integer setting.
    /// </summary>
    Int,

    /// <summary>
    /// A float setting.
    /// </summary>
    Float,

    /// <summary>
    /// A list-of-strings setting.
    /// </summary>
    ListOfStrings,

    /// <summary>
    /// A dictionary-of-strings setting.
    /// </summary>
    DictionaryOfStrings,

    /// <summary>
    /// A boolean setting.
    /// </summary>
    Bool,

    /// <summary>
    /// A byte-array setting.
    /// </summary>
    ByteArray,

    /// <summary>
    /// An enum setting.
    /// </summary>
    Enum
}
