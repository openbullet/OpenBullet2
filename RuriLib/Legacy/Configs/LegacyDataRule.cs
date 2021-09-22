namespace RuriLib.Legacy.Configs
{
    internal class LegacyDataRule
    {
        internal string SliceName { get; set; }
        internal LegacyRuleType RuleType { get; set; }
        internal string RuleString { get; set; } // Options: "Lowercase", "Uppercase", "Digit", "Symbol" or customized
    }

    internal enum LegacyRuleType
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
