namespace RuriLib.Models.Blocks;

/// <summary>
/// The delegate signature for a block action.
/// </summary>
/// <param name="block">The block instance the action operates on.</param>
public delegate void BlockActionDelegate(BlockInstance block);

/// <summary>
/// Describes an action exposed by a block.
/// </summary>
public class BlockActionInfo
{
    /// <summary>
    /// The display name of the action.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the action.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The delegate that executes the action.
    /// </summary>
    public BlockActionDelegate Delegate { get; set; } = _ => { };
}
