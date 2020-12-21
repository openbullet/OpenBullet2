using RuriLib.Models.Blocks.Parameters;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Custom
{
    public class KeycheckBlockDescriptor : BlockDescriptor
    {
        public KeycheckBlockDescriptor()
        {
            Id = "Keycheck";
            Name = Id;
            Description = "Modifies the bot's status by checking conditions";
            Category = new BlockCategory
            {
                Name = "Conditions",
                BackgroundColor = "#1e90ff",
                ForegroundColor = "#000",
                Path = "RuriLib.Blocks.Conditions",
                Namespace = "RuriLib.Functions.Conditions",
                Description = "Blocks that have to do with checking conditions"
            };
            Parameters = new Dictionary<string, BlockParameter>
            {
                { "banIfNoMatch", new BoolParameter { Name = "banIfNoMatch", DefaultValue = true } }
            };
        }
    }
}
