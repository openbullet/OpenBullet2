using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings.Interpolated
{
    public class InterpolatedDictionaryOfStringsSetting : InterpolatedSetting
    {
        public Dictionary<string, string> Value { get; set; } = new Dictionary<string, string>();
    }
}
