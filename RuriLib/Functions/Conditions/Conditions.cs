using RuriLib.Models.Conditions.Comparisons;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RuriLib.Functions.Conditions
{
    public static class Conditions
    {
        /// <summary>Compares two <see cref="bool"/> values.</summary>
        public static bool Check(bool leftTerm, BoolComparison comparison, bool rightTerm)
        {
            return comparison switch
            {
                BoolComparison.Is => leftTerm == rightTerm,
                BoolComparison.IsNot => leftTerm != rightTerm,
                _ => throw new ArgumentException("Comparison not supported", nameof(comparison))
            };
        }

        /// <summary>Compares two <see cref="string"/> values.</summary>
        public static bool Check(string leftTerm, StrComparison comparison, string rightTerm)
        {
            return comparison switch
            {
                StrComparison.EqualTo => leftTerm == rightTerm,
                StrComparison.NotEqualTo => leftTerm != rightTerm,
                StrComparison.Contains => leftTerm.Contains(rightTerm),
                StrComparison.DoesNotContain => !leftTerm.Contains(rightTerm),
                StrComparison.Exists => leftTerm != null,
                StrComparison.DoesNotExist => leftTerm == null,
                StrComparison.MatchesRegex => Regex.Match(leftTerm, rightTerm).Success,
                StrComparison.DoesNotMatchRegex => !Regex.Match(leftTerm, rightTerm).Success,
                _ => throw new ArgumentException("Comparison not supported", nameof(comparison))
            };
        }

        /// <summary>Compares a <see cref="List{T}"/> of <see cref="string"/> with a <see cref="string"/>.</summary>
        public static bool Check(List<string> leftTerm, ListComparison comparison, string rightTerm)
        {
            return comparison switch
            {
                ListComparison.Contains => leftTerm.Contains(rightTerm),
                ListComparison.DoesNotContain => !leftTerm.Contains(rightTerm),
                ListComparison.Exists => leftTerm != null,
                ListComparison.DoesNotExist => leftTerm == null,
                _ => throw new ArgumentException("Comparison not supported", nameof(comparison))
            };
        }

        /// <summary>Compares two <see cref="int"/> values.</summary>
        public static bool Check(int leftTerm, NumComparison comparison, int rightTerm)
        {
            return comparison switch
            {
                NumComparison.EqualTo => leftTerm == rightTerm,
                NumComparison.NotEqualTo => leftTerm != rightTerm,
                NumComparison.LessThan => leftTerm < rightTerm,
                NumComparison.LessThanOrEqualTo => leftTerm <= rightTerm,
                NumComparison.GreaterThan => leftTerm > rightTerm,
                NumComparison.GreaterThanOrEqualTo => leftTerm >= rightTerm,
                _ => throw new ArgumentException("Comparison not supported", nameof(comparison))
            };
        }

        /// <summary>Compares two <see cref="TimeSpan"/> values.</summary>
        public static bool Check(TimeSpan leftTerm, NumComparison comparison, TimeSpan rightTerm)
        {
            return comparison switch
            {
                NumComparison.EqualTo => leftTerm == rightTerm,
                NumComparison.NotEqualTo => leftTerm != rightTerm,
                NumComparison.LessThan => leftTerm < rightTerm,
                NumComparison.LessThanOrEqualTo => leftTerm <= rightTerm,
                NumComparison.GreaterThan => leftTerm > rightTerm,
                NumComparison.GreaterThanOrEqualTo => leftTerm >= rightTerm,
                _ => throw new ArgumentException("Comparison not supported", nameof(comparison))
            };
        }

        /// <summary>Compares two <see cref="float"/> values.</summary>
        public static bool Check(float leftTerm, NumComparison comparison, float rightTerm)
        {
            return comparison switch
            {
                NumComparison.EqualTo => leftTerm == rightTerm,
                NumComparison.NotEqualTo => leftTerm != rightTerm,
                NumComparison.LessThan => leftTerm < rightTerm,
                NumComparison.LessThanOrEqualTo => leftTerm <= rightTerm,
                NumComparison.GreaterThan => leftTerm > rightTerm,
                NumComparison.GreaterThanOrEqualTo => leftTerm >= rightTerm,
                _ => throw new ArgumentException("Comparison not supported", nameof(comparison))
            };
        }

        /// <summary>Compares a <see cref="Dictionary{TKey, TValue}"/> of (<see cref="string"/>,<see cref="string"/>) with a <see cref="string"/></summary>
        public static bool Check(Dictionary<string, string> leftTerm, DictComparison comparison, string rightTerm)
        {
            return comparison switch
            {
                DictComparison.HasKey => leftTerm.ContainsKey(rightTerm),
                DictComparison.DoesNotHaveKey => !leftTerm.ContainsKey(rightTerm),
                DictComparison.HasValue => leftTerm.ContainsValue(rightTerm),
                DictComparison.DoesNotHaveValue => !leftTerm.ContainsValue(rightTerm),
                DictComparison.Exists => leftTerm != null,
                DictComparison.DoesNotExist => leftTerm == null,
                _ => throw new ArgumentException("Comparison not supported", nameof(comparison))
            };
        }
    }
}
