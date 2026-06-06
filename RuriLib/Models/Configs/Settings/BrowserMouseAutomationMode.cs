namespace RuriLib.Models.Configs.Settings;

/// <summary>
/// Controls how mouse actions are performed for generic browser blocks.
/// </summary>
public enum BrowserMouseAutomationMode
{
    /// <summary>
    /// Uses the browser automation library's native mouse APIs directly.
    /// </summary>
    Native,

    /// <summary>
    /// Uses GhostCursor to generate human-like mouse movement and clicks.
    /// </summary>
    GhostCursor
}
