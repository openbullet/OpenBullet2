namespace RuriLib.Models.Blocks.Custom
{
    public class ParseBlockDescriptor : BlockDescriptor
    {
        public ParseBlockDescriptor()
        {
            Id = "Parse";
            Name = Id;
            Description = "Parses text from a string";
            Category = new BlockCategory
            {
                Name = "Parsing",
                BackgroundColor = "#ffd700",
                ForegroundColor = "#000",
                Path = "RuriLib.Blocks.Parsing",
                Namespace = "RuriLib.Blocks.Parsing.Methods",
                Description = "Blocks for extracting data from strings"
            };
        }
    }
}
