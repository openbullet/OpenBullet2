using System;

namespace OpenBullet2.Models.Settings
{
    public class EnumSetting : Setting
    {
        public Type EnumType { get; set; }
        public string Value { get; set; }
    }
}
