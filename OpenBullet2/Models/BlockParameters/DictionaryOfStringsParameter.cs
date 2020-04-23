using OpenBullet2.Models.Settings;
using System.Collections.Generic;

namespace OpenBullet2.Models.BlockParameters
{
    public class DictionaryOfStringsParameter : BlockParameter
    {
        public Dictionary<string, string> DefaultValue { get; set; }

        public override Setting ToSetting() => new DictionaryOfStringsSetting { Name = Name, Value = DefaultValue };
    }
}
