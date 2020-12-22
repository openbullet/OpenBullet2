using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters
{
    public class BoolParameter : BlockParameter
    {
        public bool DefaultValue { get; set; }

        public BoolParameter()
        {

        }

        public BoolParameter(string name, bool defaultValue = false)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public BoolParameter(string name, string defaultVariableName = "")
        {
            Name = name;
            DefaultVariableName = defaultVariableName;
            InputMode = SettingInputMode.Variable;
        }

        public override BlockSetting ToBlockSetting()
            => new BlockSetting { Name = Name, FixedSetting = new BoolSetting { Value = DefaultValue },
                InputMode = InputMode};
    }
}
