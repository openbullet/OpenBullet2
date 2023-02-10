using OpenBullet2.Web.Dtos.Config.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// A key of the keychain.
/// </summary>
public class KeyDto
{
    /// <summary>
    /// The left comparison term.
    /// </summary>
    public BlockSettingDto? Left { get; set; }

    /// <summary>
    /// The right comparison term.
    /// </summary>
    public BlockSettingDto? Right { get; set; }

    /// <summary>
    /// The key type.
    /// </summary>
    public KeyType KeyType { get; set; }
}

/// <summary>
/// The key type.
/// </summary>
public enum KeyType
{
    /// <summary>
    /// A string key.
    /// </summary>
    String,

    /// <summary>
    /// An integer key.
    /// </summary>
    Int,

    /// <summary>
    /// A floating point key.
    /// </summary>
    Float,

    /// <summary>
    /// A list key.
    /// </summary>
    List,

    /// <summary>
    /// A dictionary key.
    /// </summary>
    Dictionary,

    /// <summary>
    /// A boolean key.
    /// </summary>
    Bool
}
