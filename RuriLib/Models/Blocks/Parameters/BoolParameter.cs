using RuriLib.Extensions;
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
            => new()
            {
                Name = Name,
                Description = Description,
                ReadableName = PrettyName ?? Name.ToReadableName(),
                FixedSetting = new BoolSetting { Value = DefaultValue },
                InputMode = InputMode
            };
    }
}
