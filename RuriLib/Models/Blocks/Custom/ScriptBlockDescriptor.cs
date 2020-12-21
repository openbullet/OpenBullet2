using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Variables;

namespace RuriLib.Models.Blocks.Custom
{
    public class ScriptBlockDescriptor : BlockDescriptor
    {
        public ScriptBlockDescriptor()
        {
            Id = "Script";
            Name = "Script";
            Description = "This block can invoke a script in a different language, pass some variables and return some results.";
            Category = new BlockCategory
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
}
