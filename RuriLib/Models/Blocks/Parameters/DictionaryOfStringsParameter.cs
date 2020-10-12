using RuriLib.Models.Blocks.Settings;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Parameters
{
    public class DictionaryOfStringsParameter : BlockParameter
    {
        public Dictionary<string, string> DefaultValue { get; set; }

        public override BlockSetting ToBlockSetting()
            => BlockSettingFactory.CreateDictionaryOfStringsSetting(Name, DefaultValue, InputMode);
    }
}
