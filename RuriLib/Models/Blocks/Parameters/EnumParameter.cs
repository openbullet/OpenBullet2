using RuriLib.Models.Blocks.Settings;
using System;

namespace RuriLib.Models.Blocks.Parameters
{
    public class EnumParameter : BlockParameter
    {
        public Type EnumType { get; set; }
        public string DefaultValue { get; set; }

        public override BlockSetting ToBlockSetting()
            => new BlockSetting { Name = Name, FixedSetting = new EnumSetting { EnumType = EnumType, Value = DefaultValue },
                InputMode = InputMode };
    }
}
