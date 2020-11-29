using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck
{
    public class BoolKey : Key
    {
        public BoolComparison Comparison { get; set; } = BoolComparison.Is;

        public BoolKey()
        {
            Left = BlockSettingFactory.CreateBoolSetting(string.Empty);
            Left.InputMode = SettingInputMode.Variable;
            Left.InputVariableName = "data.SOURCE";
            Right = BlockSettingFactory.CreateBoolSetting(string.Empty, true);
        }
    }
}
