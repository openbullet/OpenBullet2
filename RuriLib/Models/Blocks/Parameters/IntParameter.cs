using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters
{
    public class IntParameter : BlockParameter
    {
        public int DefaultValue { get; set; }

        public override BlockSetting ToBlockSetting()
            => new BlockSetting { Name = Name, FixedSetting = new IntSetting { Value = DefaultValue },
                InputMode = InputMode };
    }
}
