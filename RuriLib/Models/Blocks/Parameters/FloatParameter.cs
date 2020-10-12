using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters
{
    public class FloatParameter : BlockParameter
    {
        public float DefaultValue { get; set; }

        public override BlockSetting ToBlockSetting()
            => new BlockSetting { Name = Name, FixedSetting = new FloatSetting { Value = DefaultValue },
                InputMode = InputMode };
    }
}
