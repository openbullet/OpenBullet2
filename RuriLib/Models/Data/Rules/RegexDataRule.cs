using System;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Data.Rules
{
    public class RegexDataRule : DataRule
    {
        public string RegexToMatch { get; set; } = "^.*$";

        public override bool IsSatisfied(string value)
            => Invert ^ Regex.IsMatch(value, RegexToMatch);
    }
}
