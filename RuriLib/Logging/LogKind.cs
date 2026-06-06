namespace RuriLib.Logging;

/// <summary>
/// Represents the severity or category of a log entry.
/// </summary>
public enum LogKind
{
    /// <summary>
    /// A custom log entry.
    /// </summary>
    Custom,
    /// <summary>
    /// An informational message.
    /// </summary>
    Info,
    /// <summary>
    /// A success message.
    /// </summary>
    Success,
    /// <summary>
    /// A warning message.
    /// </summary>
    Warning,
    /// <summary>
    /// An error message.
    /// </summary>
    Error
}
