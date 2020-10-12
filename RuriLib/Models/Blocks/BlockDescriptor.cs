using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Variables;

namespace RuriLib.Models.Blocks
{
    public class BlockDescriptor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExtraInfo { get; set; }
        
        public VariableType? ReturnType { get; set; }
        public BlockCategory Category { get; set; }
        public BlockParameter[] Parameters { get; set; }
    }
}
