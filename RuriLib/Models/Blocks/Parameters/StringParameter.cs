using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters
{
    public class StringParameter : BlockParameter
    {
        public string DefaultValue { get; set; }

        public bool MultiLine { get; set; } = false;

        public StringParameter()
        {

        }

        public StringParameter(string name, string defaultValue = "", SettingInputMode inputMode = SettingInputMode.Fixed)
        {
            Name = name;
            DefaultValue = defaultValue ?? string.Empty;
            DefaultVariableName = defaultValue ?? string.Empty;
            InputMode = inputMode;
        }

        public override BlockSetting ToBlockSetting()
            => BlockSettingFactory.CreateStringSetting(Name, DefaultValue, InputMode, MultiLine, PrettyName, Description);
    }
}
