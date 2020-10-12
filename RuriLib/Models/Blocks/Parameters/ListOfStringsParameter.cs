using RuriLib.Models.Blocks.Settings;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Parameters
{
    public class ListOfStringsParameter : BlockParameter
    {
        public List<string> DefaultValue { get; set; }

        public override BlockSetting ToBlockSetting()
            => BlockSettingFactory.CreateListOfStringsSetting(Name, DefaultValue, InputMode);
    }
}
