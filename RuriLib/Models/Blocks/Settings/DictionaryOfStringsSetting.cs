using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings
{
    public class DictionaryOfStringsSetting : Setting
    {
        public Dictionary<string, string> Value { get; set; }
    }
}
