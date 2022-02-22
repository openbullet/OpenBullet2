using RuriLib.Models.Blocks.Settings;
using System;

namespace RuriLib.Models.Blocks.Parameters
{
    public class EnumParameter : BlockParameter
    {
        public Type EnumType { get; set; }
        public string DefaultValue { get; set; }

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
            => new BlockSetting { Name = Name, FixedSetting = new EnumSetting(EnumType) { Value = DefaultValue },
                InputMode = InputMode };
    }
}
