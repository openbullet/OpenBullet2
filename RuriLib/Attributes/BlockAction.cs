using System;

namespace RuriLib.Attributes;

/// <summary>
/// Attribute used to decorate a method that is a block action. The method should take only one
/// parameter of type <see cref="Models.Blocks.BlockInstance"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BlockAction : Attribute
{
    /// <summary>
    /// The name of the action. If not specified, a name will automatically be 
    /// generated from the name of the method.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string? name;

    /// <summary>
    /// The description of what the action does.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string? description;

    /// <summary>
    /// The id of the block to which this action belongs to. Normally, the
    /// id of a block is the name of the method.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public string parentBlockId;

    /// <summary>
    /// Defines a block action.
    /// </summary>
    /// <param name="parentBlockId">The id of the block to which this action belongs to. Normally, the
    /// id of a block is the name of the method.</param>
    /// <param name="name">The name of the action. If not specified, a name will automatically be 
    /// generated from the name of the method.</param>
    /// <param name="description">The description of what the action does.</param>
    public BlockAction(string parentBlockId, string? name = null, string? description = null)
    {
        this.parentBlockId = parentBlockId;
        this.name = name;
        this.description = description;
    }
}
