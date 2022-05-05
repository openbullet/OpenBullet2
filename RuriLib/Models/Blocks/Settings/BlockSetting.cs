using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings.Interpolated;

namespace RuriLib.Models.Blocks.Settings
{
    public class BlockSetting
    {
        public SettingInputMode InputMode { get; set; } = SettingInputMode.Fixed;

        public string Name { get; set; } = string.Empty;

        private string readableName = null;
        public string ReadableName
        {
            // Failsafe in case ReadableName is never set
            get => readableName ?? Name.ToReadableName();
            set => readableName = value;
        }
        public string Description { get; set; } = null;
        public string InputVariableName { get; set; } = string.Empty;

        public Setting FixedSetting { get; set; }
        public InterpolatedSetting InterpolatedSetting { get; set; }
    }
}
