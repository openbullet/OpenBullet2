namespace RuriLib.Models.Blocks.Custom.Parse;

/// <summary>
/// Parsing modes supported by the custom parse block.
/// </summary>
public enum ParseMode
{
    /// <summary>
    /// Left-right delimiter parsing.
    /// </summary>
    LR,
    /// <summary>
    /// CSS selector parsing.
    /// </summary>
    CSS,
    /// <summary>
    /// XPath parsing.
    /// </summary>
    XPath,
    /// <summary>
    /// JSON token parsing.
    /// </summary>
    Json,
    /// <summary>
    /// Regular expression parsing.
    /// </summary>
    Regex
}
