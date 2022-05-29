using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using RuriLib.Models.Conditions.Comparisons;

namespace RuriLib.Models.Blocks.Custom.Keycheck
{
    public class DictionaryKey : Key
    {
        public DictComparison Comparison { get; set; } = DictComparison.HasKey;

        public DictionaryKey()
        {
            Left = BlockSettingFactory.CreateDictionaryOfStringsSetting("", "data.COOKIES");
            Left.InputVariableName = "data.HEADERS";
            Right = BlockSettingFactory.CreateStringSetting("");
        }
    }
}
