using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;
using System;

namespace RuriLib.Models.Blocks.Parameters;

/// <summary>
/// A parameter of a block.
/// </summary>
public abstract class BlockParameter
{
    /// <summary></summary>
    protected BlockParameter(string name)
    {
        Name = name;
    }
    
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// The assigned name of the parameter.
    /// </summary>
    public string? AssignedName { get; set; }
    
    /// <summary>
    /// The pretty name of the parameter.
    /// </summary>
    public string PrettyName => AssignedName ?? Name.ToReadableName();
    
    /// <summary>
    /// The description of the parameter.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// The input mode of the parameter.
    /// </summary>
    public SettingInputMode InputMode { get; set; } = SettingInputMode.Fixed;
    
    /// <summary>
    /// The default variable name for the parameter.
    /// </summary>
    public string DefaultVariableName { get; set; } = string.Empty;

    /// <summary>
    /// Converts the parameter to a block setting.
    /// </summary>
    public virtual BlockSetting ToBlockSetting()
        => throw new NotImplementedException();
}
