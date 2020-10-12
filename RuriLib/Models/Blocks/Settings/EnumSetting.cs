using System;

namespace RuriLib.Models.Blocks.Settings
{
    public class EnumSetting : Setting
    {
        public Type EnumType { get; set; }
        public string Value { get; set; }
    }
}
