using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;
using System;

namespace RuriLib.Models.Blocks.Parameters
{
    public class EnumParameter : BlockParameter
    {
        public Type EnumType { get; set; }
        public string DefaultValue { get; set; }
        public string[] Options => Enum.GetNames(EnumType);

        public EnumParameter()
        {

        }

        public EnumParameter(string name, Type enumType, string defaultValue)
        {
            Name = name;
            EnumType = enumType;
            DefaultValue = defaultValue;
        }

        public override BlockSetting ToBlockSetting()
            => new()
            {
                Name = Name,
                Description = Description,
                ReadableName = PrettyName ?? Name.ToReadableName(),
                FixedSetting = new EnumSetting { EnumType = EnumType, Value = DefaultValue },
                InputMode = InputMode
            };
    }
}
