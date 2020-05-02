using RuriLib.Functions.Conditions;

namespace OpenBullet2.Models.Keycheck
{
    public class NumericKey : Key
    {
        public NumericComparison Comparison { get; set; } = NumericComparison.EqualTo;
    }
}
