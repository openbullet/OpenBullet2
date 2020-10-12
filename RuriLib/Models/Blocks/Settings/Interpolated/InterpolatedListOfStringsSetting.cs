using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Settings.Interpolated
{
    public class InterpolatedListOfStringsSetting : InterpolatedSetting
    {
        public List<string> Value { get; set; } = new List<string>();
    }
}
