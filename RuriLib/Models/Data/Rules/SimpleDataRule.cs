using System;
using System.Linq;

namespace RuriLib.Models.Data.Rules
{
    public class SimpleDataRule : DataRule
    {
        public StringRule Comparison { get; set; } = StringRule.EqualTo;
        public string StringToCompare { get; set; } = string.Empty;
        public bool CaseSensitive { get; set; } = true;

        public override bool IsSatisfied(string value)
        {
            var cs = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            return Invert ^ Comparison switch
            {
                StringRule.EqualTo => value.Equals(StringToCompare, cs),
                StringRule.Contains => value.Contains(StringToCompare, cs),
                StringRule.LongerThan => value.Length > int.Parse(StringToCompare),
                StringRule.ShorterThan => value.Length < int.Parse(StringToCompare),
                StringRule.ContainsAll => StringToCompare.All(c => value.IndexOf(c, cs) != -1),
                StringRule.ContainsAny => StringToCompare.Any(c => value.IndexOf(c, cs) != -1),
                StringRule.StartsWith => value.StartsWith(StringToCompare, cs),
                StringRule.EndsWith => value.EndsWith(StringToCompare, cs),
                _ => throw new NotImplementedException()
            };
        }
    }

    public enum StringRule
    {
        EqualTo,
        Contains,
        LongerThan,
        ShorterThan,
        ContainsAll,
        ContainsAny,
        StartsWith,
        EndsWith,
    }
}
