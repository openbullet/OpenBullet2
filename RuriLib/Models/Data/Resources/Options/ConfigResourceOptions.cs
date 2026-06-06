namespace RuriLib.Models.Data.Resources.Options;

/// <summary>
/// Base options shared by all config resources.
/// </summary>
public abstract class ConfigResourceOptions
{
    /// <summary>
    /// The unique name of the resource.
    /// </summary>
    public string Name { get; set; } = "resource";
}
