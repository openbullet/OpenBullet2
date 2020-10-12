using RuriLib.Models.Variables;

namespace RuriLib.Models.Blocks.Custom.Script
{
    public class OutputVariable
    {
        public VariableType Type { get; set; } = VariableType.String;
        public string Name { get; set; } = string.Empty;
    }
}
