namespace RuriLib.Models.Blocks.Custom.Keycheck;

/// <summary>
/// Logical modes for combining keys inside a keychain.
/// </summary>
public enum KeychainMode
{
    /// <summary>
    /// Any key may match.
    /// </summary>
    OR,
    /// <summary>
    /// All keys must match.
    /// </summary>
    AND
}
