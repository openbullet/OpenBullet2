using RuriLib.Legacy.Functions.Conditions;

namespace RuriLib.Legacy.Models
{
    /// <summary>
    /// Represents a Key in a KeyChain.
    /// </summary>
    public class Key
    {
        /// <summary>The left-hand term for the comparison.</summary>
        public string LeftTerm { get; set; } = "<SOURCE>";

        /// <summary>The comparison operator.</summary>
        public Comparer Comparer { get; set; } = Comparer.Contains;

        /// <summary>The right-hand term of the comparison.</summary>
        public string RightTerm { get; set; } = "";

        /// <summary>
        /// Checks the comparison between left and right member.
        /// </summary>
        public bool CheckKey(LSGlobals ls)
        {
            try
            {
                return Condition.ReplaceAndVerify(LeftTerm, Comparer, RightTerm, ls);
            }
            catch
            {
                // Return false if e.g. we can't parse the number for a LessThan/GreaterThan comparison.
                return false;
            }
        }
    }
}
