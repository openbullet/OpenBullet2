using OpenBullet2.Enums;

namespace OpenBullet2.Models.Settings
{
    public class BlockSetting
    {
        public InputMode InputMode { get; set; } = InputMode.Fixed;
        
        public string InputVariableName { get; set; } = "";
        public Setting FixedSetting { get; set; }
        public string InterpolatedString { get; set; } = "";
    }
}
