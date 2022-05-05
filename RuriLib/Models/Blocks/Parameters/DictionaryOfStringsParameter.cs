using RuriLib.Models.Blocks.Settings;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Parameters
{
    public class DictionaryOfStringsParameter : BlockParameter
    {
        public Dictionary<string, string> DefaultValue { get; set; }

        public DictionaryOfStringsParameter()
        {

        }

        public DictionaryOfStringsParameter(string name, Dictionary<string, string> defaultValue = null, SettingInputMode inputMode = SettingInputMode.Fixed)
        {
            Name = name;
            DefaultValue = defaultValue ?? new Dictionary<string, string>();
            DefaultVariableName = string.Empty;
            InputMode = SettingInputMode.Fixed;
        }

        public DictionaryOfStringsParameter(string name, string defaultVariableName = "")
        {
            Name = name;
            DefaultVariableName = defaultVariableName;
            InputMode = SettingInputMode.Variable;
        }

        public override BlockSetting ToBlockSetting()
            => BlockSettingFactory.CreateDictionaryOfStringsSetting(Name, DefaultValue, InputMode, PrettyName, Description);
    }
}
