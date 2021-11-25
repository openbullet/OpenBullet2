namespace RuriLib.Legacy.Configs
{
    public class LegacyDataRule
    {
        public string SliceName { get; set; }
        public LegacyRuleType RuleType { get; set; }
        public string RuleString { get; set; } // Options: "Lowercase", "Uppercase", "Digit", "Symbol" or customized
    }

    public enum LegacyRuleType
    {
        /// <summary>The slice must contain the given characters.</summary>
        MustContain,

        /// <summary>The slice must not contain the given characters.</summary>
        MustNotContain,

        /// <summary>The slice's length must be greater or equal to a given number.</summary>
        MinLength,

        /// <summary>The slice's length must be smaller or equal to a given number.</summary>
        MaxLength,

        /// <summary>The slice must match a given regex pattern.</summary>
        MustMatchRegex
    }
}
