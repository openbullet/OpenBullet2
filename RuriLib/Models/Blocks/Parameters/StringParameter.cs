using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters
{
    public class StringParameter : BlockParameter
    {
        public string DefaultValue { get; set; }

        public override BlockSetting ToBlockSetting()
            => BlockSettingFactory.CreateStringSetting(Name, DefaultValue, InputMode);
    }
}
