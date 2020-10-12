using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters
{
    public class ByteArrayParameter : BlockParameter
    {
        public byte[] DefaultValue { get; set; }

        public override BlockSetting ToBlockSetting()
            => new BlockSetting { Name = Name, FixedSetting = new ByteArraySetting { Value = DefaultValue },
                InputMode = InputMode };
    }
}
