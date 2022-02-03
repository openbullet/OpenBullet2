using Newtonsoft.Json;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Variables;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks
{
    public class BlockDescriptor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExtraInfo { get; set; }
        public string AssemblyFullName { get; set; }

        public VariableType? ReturnType { get; set; }
        public BlockCategory Category { get; set; }
        public Dictionary<string, BlockParameter> Parameters { get; set; } = new Dictionary<string, BlockParameter>();

        [JsonIgnore]
        public List<BlockActionInfo> Actions { get; set; } = new List<BlockActionInfo>();

        [JsonIgnore]
        public Dictionary<string, BlockImageInfo> Images { get; set; } = new Dictionary<string, BlockImageInfo>();
    }
}
