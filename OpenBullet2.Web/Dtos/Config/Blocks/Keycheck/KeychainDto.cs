using RuriLib.Models.Blocks.Custom.Keycheck;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;

/// <summary>
/// The collection of keys to check.
/// </summary>
public class KeychainDto
{
    /// <summary>
    /// The list of keys.
    /// </summary>
    public List<object> Keys { get; set; } = new();

    /// <summary>
    /// How the keys should be checked together.
    /// </summary>
    public KeychainMode Mode { get; set; }

    /// <summary>
    /// The status that will be set when the keychain's condition
    /// is verified.
    /// </summary>
    public string ResultStatus { get; set; } = string.Empty;
}
