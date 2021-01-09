using RuriLib.Models.Blocks.Settings.Interpolated;

namespace RuriLib.Models.Blocks.Settings
{
    public class BlockSetting
    {
        public SettingInputMode InputMode { get; set; } = SettingInputMode.Fixed;

        public string Name { get; set; } = string.Empty;
        public string InputVariableName { get; set; } = string.Empty;

        public Setting FixedSetting { get; set; }
        public InterpolatedSetting InterpolatedSetting { get; set; }
    }
}
