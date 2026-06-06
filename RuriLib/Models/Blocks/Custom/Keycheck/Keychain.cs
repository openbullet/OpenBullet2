using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Custom.Keycheck;

/// <summary>
/// Represents a group of keys evaluated together.
/// </summary>
public class Keychain
{
    /// <summary>
    /// Gets or sets the keys in the chain.
    /// </summary>
    public List<Key> Keys { get; set; } = [];
    /// <summary>
    /// Gets or sets how the keys are combined.
    /// </summary>
    public KeychainMode Mode { get; set; } = KeychainMode.OR;
    /// <summary>
    /// Gets or sets the status assigned when the chain matches.
    /// </summary>
    public string ResultStatus { get; set; } = "SUCCESS";
}
