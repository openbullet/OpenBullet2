namespace RuriLib.Models.Blocks;

/// <summary>
/// A descriptor for a block generated from an exposed C# method.
/// </summary>
public class AutoBlockDescriptor : BlockDescriptor
{
    /// <summary>
    /// The C# method name to call when generating code for this block.
    /// </summary>
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the underlying method is asynchronous.
    /// </summary>
    public bool Async { get; set; }
}
