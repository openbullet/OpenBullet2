namespace RuriLib.Models.Blocks
{
    public class LoliCodeBlockDescriptor : BlockDescriptor
    {
        public LoliCodeBlockDescriptor()
        {
            Id = "loliCode";
            Name = "LoliCode";
            Description = "This block can hold a LoliCode script";
            Category = new BlockCategory
            {
                Description = "Category for the LoliCode script block",
                BackgroundColor = "#303030",
                ForegroundColor = "#fff",
                Name = "LoliCode"
            };
        }
    }
}
