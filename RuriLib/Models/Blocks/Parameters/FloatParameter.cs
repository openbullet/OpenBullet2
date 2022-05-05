using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters
{
    public class FloatParameter : BlockParameter
    {
        public float DefaultValue { get; set; }

        public FloatParameter()
        {

        }

        public FloatParameter(string name, float defaultValue = 0)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public FloatParameter(string name, string defaultVariableName = "")
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
                FixedSetting = new FloatSetting { Value = DefaultValue },
                InputMode = InputMode
            };
    }
}
