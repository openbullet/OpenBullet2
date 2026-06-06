using System.Collections.Generic;

namespace RuriLib.Models.Configs.Settings;

/// <summary>
/// Describes a custom input consumed by a config.
/// </summary>
public class CustomInput
{
    /// <summary>
    /// The description shown to the user.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The variable name that will receive the answer.
    /// </summary>
    public string VariableName { get; set; } = string.Empty;

    /// <summary>
    /// The default answer used when no explicit value is provided.
    /// </summary>
    public string DefaultAnswer { get; set; } = string.Empty;
}

/// <summary>
/// Groups all custom inputs of a config.
/// </summary>
public class InputSettings
{
    /// <summary>
    /// The custom inputs available to the config.
    /// </summary>
    public List<CustomInput> CustomInputs { get; set; } = [];
}
