namespace RuriLib.Models.Blocks;

/// <summary>
/// Descriptor for the raw LoliCode script block.
/// </summary>
public class LoliCodeBlockDescriptor : BlockDescriptor
{
    /// <summary>
    /// Initializes a new <see cref="LoliCodeBlockDescriptor"/>.
    /// </summary>
    public LoliCodeBlockDescriptor()
    {
        Id = "loliCode";
        Name = "LoliCode";
        Description = "This block can hold a LoliCode script";
        Category = new()
        {
            Description = "Category for the LoliCode script block",
            BackgroundColor = "#303030",
            ForegroundColor = "#fff",
            Name = "LoliCode"
        };
    }
}
