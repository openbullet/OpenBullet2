using RuriLib.Models.Blocks.Parameters;

namespace RuriLib.Models.Blocks.Custom;

/// <summary>
/// Descriptor for the keycheck block.
/// </summary>
public class KeycheckBlockDescriptor : BlockDescriptor
{
    /// <summary>
    /// Initializes a new <see cref="KeycheckBlockDescriptor"/>.
    /// </summary>
    public KeycheckBlockDescriptor()
    {
        Id = "Keycheck";
        Name = Id;
        Description = "Modifies the bot's status by checking conditions";
        Category = new()
        {
            Name = "Conditions",
            BackgroundColor = "#1e90ff",
            ForegroundColor = "#000",
            Path = "RuriLib.Blocks.Conditions",
            Namespace = "RuriLib.Blocks.Conditions.Methods",
            Description = "Blocks that have to do with checking conditions"
        };
        Parameters = new()
        {
            ["banIfNoMatch"] = new BoolParameter("banIfNoMatch", true)
        };
    }
}
