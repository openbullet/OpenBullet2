using RuriLib.Helpers;
using RuriLib.Models.Blocks.Settings.Interpolated;

namespace RuriLib.Models.Blocks.Settings
{
    public class BlockSetting
    {
        public SettingInputMode InputMode { get; set; } = SettingInputMode.Fixed;

        public string Name { get; set; } = "";

        private string inputVariableName = "";
        public string InputVariableName 
        {
            get => inputVariableName; 
            set => inputVariableName = VariableNames.MakeValid(value);
        }

        public Setting FixedSetting { get; set; }
        public InterpolatedSetting InterpolatedSetting { get; set; }
    }
}
