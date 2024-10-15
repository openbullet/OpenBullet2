using System;

namespace RuriLib.Attributes;

/// <summary>
/// Attribute used to decorate a method that can be turned into an auto block.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class Block : Attribute
{
    /// <summary>
    /// The name of the block. If not specified, a name will be automatically
    /// generated from the name of the method.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string? name = null;

    /// <summary>
    /// The description of what the block does.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string description;

    /// <summary>
    /// Any extra information that is too long to fit the short and concise description.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string? extraInfo = null;

    /// <summary>
    /// Creates a <see cref="Block"/> attribute given the <paramref name="description"/> of what the block does.
    /// The name of the block will be automatically generated unless explicitly set in the <see cref="name"/> field.
    /// </summary>
    public Block(string description)
    {
        this.description = description;
    }
}
