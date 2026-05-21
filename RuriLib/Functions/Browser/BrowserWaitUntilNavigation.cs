namespace RuriLib.Functions.Browser;

/// <summary>
/// Defines which browser event should mark a navigation as completed.
/// </summary>
public enum BrowserWaitUntilNavigation
{
    /// <summary>
    /// Wait for the load event.
    /// </summary>
    Load,

    /// <summary>
    /// Wait for the DOMContentLoaded event.
    /// </summary>
    DOMContentLoaded,

    /// <summary>
    /// Wait until there are no more than 0 network connections for a short period.
    /// </summary>
    Networkidle0,

    /// <summary>
    /// Wait until there are no more than 2 network connections for a short period.
    /// </summary>
    Networkidle2
}
