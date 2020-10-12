using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck
{
    public class FloatKey : Key
    {
        public NumComparison Comparison { get; set; } = NumComparison.EqualTo;

        public FloatKey()
        {
            Left = new BlockSetting() { InputMode = SettingInputMode.Variable, FixedSetting = new FloatSetting() };
            Right = new BlockSetting() { FixedSetting = new FloatSetting() };
        }
    }
}
