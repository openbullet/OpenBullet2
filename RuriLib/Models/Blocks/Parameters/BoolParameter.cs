using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters
{
    public class BoolParameter : BlockParameter
    {
        public bool DefaultValue { get; set; }

        public override BlockSetting ToBlockSetting()
            => new BlockSetting { Name = Name, FixedSetting = new BoolSetting { Value = DefaultValue },
                InputMode = InputMode};
    }
}
