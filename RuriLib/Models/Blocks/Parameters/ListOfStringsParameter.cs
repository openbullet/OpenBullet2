using RuriLib.Models.Blocks.Settings;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Parameters
{
    public class ListOfStringsParameter : BlockParameter
    {
        public List<string> DefaultValue { get; set; }

        public ListOfStringsParameter()
        {

        }

        public ListOfStringsParameter(string name, List<string> defaultValue = null, SettingInputMode inputMode = SettingInputMode.Fixed)
        {
            Name = name;
            DefaultValue = defaultValue ?? new List<string>();
            DefaultVariableName = string.Empty;
            InputMode = SettingInputMode.Fixed;
        }

        public ListOfStringsParameter(string name, string defaultVariableName = "")
        {
            Name = name;
            DefaultVariableName = defaultVariableName;
            InputMode = SettingInputMode.Variable;
        }

        public override BlockSetting ToBlockSetting()
            => BlockSettingFactory.CreateListOfStringsSetting(Name, DefaultValue, InputMode, PrettyName, Description);
    }
}
