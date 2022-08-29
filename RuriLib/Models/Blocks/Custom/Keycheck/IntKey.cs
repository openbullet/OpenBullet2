using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck
{
    public class IntKey : Key
    {
        public NumComparison Comparison { get; set; } = NumComparison.EqualTo;

        public IntKey()
        {
            Left = new BlockSetting {
                InputMode = SettingInputMode.Variable, FixedSetting = new IntSetting(), InputVariableName = "data.RESPONSECODE"
            };
            Right = new BlockSetting() { FixedSetting = new IntSetting() };
        }
    }
}
