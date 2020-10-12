using RuriLib.Models.Blocks.Settings;
using System;

namespace RuriLib.Models.Blocks.Parameters
{
    public abstract class BlockParameter
    {
        public string Name { get; set; }
        public SettingInputMode InputMode { get; set; } = SettingInputMode.Fixed;
        public string DefaultVariableName { get; set; } = string.Empty;

        public virtual BlockSetting ToBlockSetting()
             => throw new NotImplementedException();
    }
}
