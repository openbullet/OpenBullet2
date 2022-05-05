using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;

namespace RuriLib.Models.Blocks.Parameters
{
    public class IntParameter : BlockParameter
    {
        public int DefaultValue { get; set; }

        public IntParameter()
        {

        }

        public IntParameter(string name, int defaultValue = 0)
        {
            Name = name;
            DefaultValue = defaultValue;
        }

        public IntParameter(string name, string defaultVariableName = "")
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
                FixedSetting = new IntSetting { Value = DefaultValue },
                InputMode = InputMode
            };
    }
}
