using System;

namespace RuriLib.Attributes;

/// <summary>
/// Attribute used to decorate a parameter of a block method to indicate it should be initialized
/// as a setting of type variable, optionally with the given default variable name.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class Variable : Attribute
{
    /// <summary>
    /// The default variable name to assign as input to this parameter, e.g. data.SOURCE
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string? defaultVariableName;

    /// <summary>
    /// Marks the parameter as a variable with no default name.
    /// </summary>
    public Variable()
    {

    }

    /// <summary>
    /// Marks the parameter as a variable with the given default name.
    /// </summary>
    public Variable(string defaultVariableName)
    {
        this.defaultVariableName = defaultVariableName;
    }
}
