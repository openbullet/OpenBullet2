using RuriLib.Functions.Conditions;

namespace OpenBullet2.Models.Keycheck
{
    public class ListKey : Key
    {
        public ListComparison Comparison { get; set; } = ListComparison.Contains;
    }
}
