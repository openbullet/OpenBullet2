namespace RuriLib.Legacy.Functions.Conditions
{
    /// <summary>
    /// Represents a condition of a keycheck.
    /// </summary>
    public struct KeycheckCondition
    {
        /// <summary>
        /// The left term.
        /// </summary>
        public string Left;

        /// <summary>
        /// The comparison operator.
        /// </summary>
        public Comparer Comparer;

        /// <summary>
        /// The right term.
        /// </summary>
        public string Right;
    }
}
