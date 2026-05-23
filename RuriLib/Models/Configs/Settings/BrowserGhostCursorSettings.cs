namespace RuriLib.Models.Configs.Settings;

/// <summary>
/// Configures GhostCursor behavior for generic browser blocks.
/// </summary>
public class BrowserGhostCursorSettings
{
    /// <summary>
    /// The optional GhostCursor movement speed hint.
    /// </summary>
    public double? MoveSpeed { get; set; }

    /// <summary>
    /// The optional delay in milliseconds after GhostCursor move actions.
    /// </summary>
    public int? MoveDelay { get; set; }

    /// <summary>
    /// Whether GhostCursor move delays should be randomized.
    /// </summary>
    public bool RandomizeMoveDelay { get; set; }

    /// <summary>
    /// The optional delay in milliseconds between GhostCursor path points.
    /// </summary>
    public int? DelayPerStep { get; set; }

    /// <summary>
    /// The optional GhostCursor scroll speed from 0 to 100.
    /// </summary>
    public double? ScrollSpeed { get; set; }

    /// <summary>
    /// The optional delay in milliseconds after GhostCursor scroll actions.
    /// </summary>
    public int? ScrollDelay { get; set; }

    /// <summary>
    /// The optional delay in milliseconds before GhostCursor presses the mouse button.
    /// </summary>
    public int? Hesitate { get; set; }

    /// <summary>
    /// The optional delay in milliseconds between GhostCursor mouse down and mouse up.
    /// </summary>
    public int? WaitForClick { get; set; }

    /// <summary>
    /// The optional maximum number of GhostCursor retry attempts if an element moves.
    /// </summary>
    public int? MaxTries { get; set; }

    /// <summary>
    /// The optional distance threshold that triggers GhostCursor overshoot correction.
    /// </summary>
    public double? OvershootThreshold { get; set; }
}
