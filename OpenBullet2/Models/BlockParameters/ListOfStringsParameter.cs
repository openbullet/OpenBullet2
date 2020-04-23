using OpenBullet2.Models.Settings;
using System.Collections.Generic;

namespace OpenBullet2.Models.BlockParameters
{
    public class ListOfStringsParameter : BlockParameter
    {
        public List<string> DefaultValue { get; set; }

        public override Setting ToSetting() => new ListOfStringsSetting { Name = Name, Value = DefaultValue };
    }
}
