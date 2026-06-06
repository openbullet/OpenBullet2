namespace RuriLib.Models.Configs;

/// <summary>
/// Enumerates the supported config representations.
/// </summary>
public enum ConfigMode
{
    /// <summary>
    /// Visual block stack mode.
    /// </summary>
    Stack,

    /// <summary>
    /// LoliCode script mode.
    /// </summary>
    LoliCode,

    /// <summary>
    /// Raw C# script mode.
    /// </summary>
    CSharp,

    /// <summary>
    /// Compiled DLL mode.
    /// </summary>
    DLL,

    /// <summary>
    /// Legacy LoliScript mode.
    /// </summary>
    Legacy
}
