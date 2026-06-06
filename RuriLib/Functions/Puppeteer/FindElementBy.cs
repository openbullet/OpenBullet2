namespace RuriLib.Functions.Puppeteer;

/// <summary>
/// Identifies how a Puppeteer element should be located.
/// </summary>
public enum FindElementBy
{
    /// <summary>
    /// Locate by element id.
    /// </summary>
    Id,
    /// <summary>
    /// Locate by CSS class.
    /// </summary>
    Class,
    /// <summary>
    /// Locate by CSS selector.
    /// </summary>
    Selector,
    /// <summary>
    /// Locate by XPath.
    /// </summary>
    XPath
}
