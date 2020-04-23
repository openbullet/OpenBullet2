using OpenBullet2.Models.Settings;
using System;

namespace OpenBullet2.Models.BlockParameters
{
    public class EnumParameter : BlockParameter
    {
        public Type EnumType { get; set; }
        public string DefaultValue { get; set; }

        public override Setting ToSetting() => new EnumSetting { Name = Name, EnumType = EnumType, Value = DefaultValue };
    }
}
