using RuriLib.Functions.Conditions;

namespace OpenBullet2.Models.Keycheck
{
    public class StringKey : Key
    {
        public StringComparison Comparison { get; set; } = StringComparison.Contains;
    }
}
