namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// DTO that contains a config's GhostCursor settings.
/// </summary>
public class ConfigGhostCursorSettingsDto
{
    /// <summary>
    /// The optional GhostCursor movement speed hint.
    /// </summary>
    public double? MoveSpeed { get; set; } = null;

    /// <summary>
    /// The optional delay in milliseconds after GhostCursor move actions.
    /// </summary>
    public int? MoveDelay { get; set; } = null;

    /// <summary>
    /// Whether GhostCursor move delays should be randomized.
    /// </summary>
    public bool RandomizeMoveDelay { get; set; } = false;

    /// <summary>
    /// The optional delay in milliseconds between GhostCursor path points.
    /// </summary>
    public int? DelayPerStep { get; set; } = null;

    /// <summary>
    /// The optional GhostCursor scroll speed from 0 to 100.
    /// </summary>
    public double? ScrollSpeed { get; set; } = null;

    /// <summary>
    /// The optional delay in milliseconds after GhostCursor scroll actions.
    /// </summary>
    public int? ScrollDelay { get; set; } = null;

    /// <summary>
    /// The optional delay in milliseconds before GhostCursor presses the mouse button.
    /// </summary>
    public int? Hesitate { get; set; } = null;

    /// <summary>
    /// The optional delay in milliseconds between GhostCursor mouse down and mouse up.
    /// </summary>
    public int? WaitForClick { get; set; } = null;

    /// <summary>
    /// The optional maximum number of GhostCursor retry attempts if an element moves.
    /// </summary>
    public int? MaxTries { get; set; } = null;

    /// <summary>
    /// The optional distance threshold that triggers GhostCursor overshoot correction.
    /// </summary>
    public double? OvershootThreshold { get; set; } = null;
}
