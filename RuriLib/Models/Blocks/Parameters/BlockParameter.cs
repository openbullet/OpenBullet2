using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Variables;
using System;

namespace RuriLib.Models.Blocks.Parameters
{
    public abstract class BlockParameter
    {
        public string Name { get; set; }
        public string AssignedName { get; set; }
        public string PrettyName => AssignedName ?? Name.ToReadableName();
        public string Description { get; set; } = null;
        public SettingInputMode InputMode { get; set; } = SettingInputMode.Fixed;
        public string DefaultVariableName { get; set; } = string.Empty;
        public string Type => GetType().Name;

        public virtual BlockSetting ToBlockSetting()
             => throw new NotImplementedException();
    }
}
