using RuriLib.Legacy.Blocks;
using RuriLib.Legacy.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Legacy.Functions.Conditions
{
    /// <summary>
    /// Static Class used to check if a condition is true or false.
    /// </summary>
    public static class Condition
    {
        /// <summary>
        /// Replaces the values and verifies if a condition is true or false.
        /// </summary>
        public static bool ReplaceAndVerify(string left, Comparer comparer, string right, LSGlobals ls)
            => ReplaceAndVerify(new KeycheckCondition() { Left = left, Comparer = comparer, Right = right }, ls);

        /// <summary>
        /// Replaces the values and verifies if a condition is true or false.
        /// </summary>
        public static bool ReplaceAndVerify(KeycheckCondition kcCond, LSGlobals ls)
        {
            var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol; // Needed when comparing values with a currency symbol
            var provider = new CultureInfo("en-US");
            var L = BlockBase.ReplaceValuesRecursive(kcCond.Left, ls); // The left-hand term can accept recursive values like <LIST[*]>
            var r = BlockBase.ReplaceValues(kcCond.Right, ls); // The right-hand term cannot

            switch (kcCond.Comparer)
            {
                case Comparer.EqualTo:
                    return L.Any(l => l == r);

                case Comparer.NotEqualTo:
                    return L.Any(l => l != r);

                case Comparer.GreaterThan:
                    return L.Any(l => decimal.Parse(l.Replace(',', '.'), style, provider) > decimal.Parse(r.Replace(',', '.'), style, provider));

                case Comparer.LessThan:
                    return L.Any(l => decimal.Parse(l.Replace(',', '.'), style, provider) < decimal.Parse(r.Replace(',', '.'), style, provider));

                case Comparer.Contains:
                    return L.Any(l => l.Contains(r));

                case Comparer.DoesNotContain:
                    return L.Any(l => !l.Contains(r));

                case Comparer.Exists:
                    return L.Any(l => l != kcCond.Left); // Returns true if any replacement took place

                case Comparer.DoesNotExist:
                    return L.All(l => l == kcCond.Left); // Returns true if no replacement took place

                case Comparer.MatchesRegex:
                    return L.Any(l => Regex.Match(l, r).Success);

                case Comparer.DoesNotMatchRegex:
                    return L.Any(l => !Regex.Match(l, r).Success);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Verifies if a condition is true or false (without replacing the values).
        /// </summary>
        /// <param name="kcCond">The keycheck condition struct</param>
        /// <returns>Whether the comparison is verified or not.</returns>
        public static bool Verify(KeycheckCondition kcCond)
        {
            var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol; // Needed when comparing values with a currency symbol
            var provider = new CultureInfo("en-US");

            switch (kcCond.Comparer)
            {
                case Comparer.EqualTo:
                    return kcCond.Left == kcCond.Right;

                case Comparer.NotEqualTo:
                    return kcCond.Left != kcCond.Right;

                case Comparer.GreaterThan:
                    return decimal.Parse(kcCond.Left.Replace(',', '.'), style, provider) > decimal.Parse(kcCond.Right.Replace(',', '.'), style, provider);

                case Comparer.LessThan:
                    return decimal.Parse(kcCond.Left.Replace(',', '.'), style, provider) < decimal.Parse(kcCond.Right.Replace(',', '.'), style, provider);

                case Comparer.Contains:
                    return kcCond.Left.Contains(kcCond.Right);

                case Comparer.DoesNotContain:
                    return !kcCond.Left.Contains(kcCond.Right);

                case Comparer.Exists:
                case Comparer.DoesNotExist:
                    throw new NotSupportedException("Exists and DoesNotExist operators are only supported in the ReplaceAndVerify method.");

                case Comparer.MatchesRegex:
                    return Regex.Match(kcCond.Left, kcCond.Right).Success;

                case Comparer.DoesNotMatchRegex:
                    return !Regex.Match(kcCond.Left, kcCond.Right).Success;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Verifies if all the provided conditions are true (after replacing).
        /// </summary>
        public static bool ReplaceAndVerifyAll(KeycheckCondition[] conditions, LSGlobals ls)
            => conditions.All(c => ReplaceAndVerify(c, ls));

        /// <summary>
        /// Verifies if all the provided conditions are true (without replacing).
        /// </summary>
        /// <param name="conditions">The keycheck conditions</param>
        /// <returns>True if all the conditions are verified.</returns>
        public static bool VerifyAll(KeycheckCondition[] conditions)
            => conditions.All(Verify);

        /// <summary>
        /// Verifies if at least one of the provided conditions is true (after replacing).
        /// </summary>
        public static bool ReplaceAndVerifyAny(KeycheckCondition[] conditions, LSGlobals ls)
            => conditions.Any(c => ReplaceAndVerify(c, ls));

        /// <summary>
        /// Verifies if at least one of the provided conditions is true (without replacing).
        /// </summary>
        /// <param name="conditions">The keycheck conditions</param>
        /// <returns>True if any condition is verified.</returns>
        public static bool VerifyAny(KeycheckCondition[] conditions)
            => conditions.Any(Verify);
    }

    
}
