namespace RuriLib.Legacy.Functions.Conditions
{
    /// <summary>
    /// The condition on which to base the outcome of a comparison.
    /// </summary>
    public enum Comparer
    {
        /// <summary>A is less than B.</summary>
        LessThan,

        /// <summary>A is greater than B.</summary>
        GreaterThan,

        /// <summary>A is equal to B.</summary>
        EqualTo,

        /// <summary>A is not equal to B.</summary>
        NotEqualTo,

        /// <summary>A contains B.</summary>
        Contains,

        /// <summary>A does not contain B.</summary>
        DoesNotContain,

        /// <summary>Whether any variable can be replaced inside the string.</summary>
        Exists,

        /// <summary>Whether no variable can be replaced inside the string.</summary>
        DoesNotExist,

        /// <summary>A matches regex pattern B.</summary>
        MatchesRegex,

        /// <summary>A does not match regex pattern B.</summary>
        DoesNotMatchRegex
    }
}
