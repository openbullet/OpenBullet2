namespace RuriLib.Models.Blocks.Custom;

/// <summary>
/// Descriptor for the script block.
/// </summary>
public class ScriptBlockDescriptor : BlockDescriptor
{
    /// <summary>
    /// Initializes a new <see cref="ScriptBlockDescriptor"/>.
    /// </summary>
    public ScriptBlockDescriptor()
    {
        Id = "Script";
        Name = "Script";
        Description = "This block can invoke a script in a different language, pass some variables and return some results.";
        Category = new()
        {
            Name = "Interop",
            BackgroundColor = "#ddadaf",
            ForegroundColor = "#000",
            Path = "RuriLib.Blocks.Interop",
            Namespace = "RuriLib.Blocks.Interop.Methods",
            Description = "Blocks for interoperability with other programs"
        };
    }
}
