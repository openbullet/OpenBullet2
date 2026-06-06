namespace RuriLib.Models.Environment;

/// <summary>
/// Defines a custom hit or execution status.
/// </summary>
public class CustomStatus
{
    /// <summary>
    /// The display name of the status.
    /// </summary>
    public string Name { get; set; } = "CUSTOM";

    /// <summary>
    /// The color associated with the status.
    /// </summary>
    public string Color { get; set; } = "#FFA500";
}
