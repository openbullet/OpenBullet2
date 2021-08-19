using RuriLib.Models.Data.Rules;
using RuriLib.Models.Environment;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Data
{
    public class DataLine
    {
        /// <summary>The actual content of the line.</summary>
        public string Data { get; set; }

        /// <summary>The WordlistType of the Wordlist the line belongs to.</summary>
        public WordlistType Type { get; set; }

        /// <summary>The amount of times the data has been retried.</summary>
        public int Retries { get; set; } = 0;

        /// <summary>Whether the data line respects the regex verification (if set to verify).</summary>
        public bool IsValid => !Type.Verify || Regex.Match(Data, Type.Regex).Success;

        /// <summary>
        /// Creates a CData object given some <paramref name="data"/> and the <paramref name="wordlistType"/>.
        /// </summary>
        public DataLine(string data, WordlistType wordlistType)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Type = wordlistType ?? throw new ArgumentNullException(nameof(wordlistType));
        }

        /// <summary>
        /// Gets all the variables that need to be set after slicing the data line.
        /// </summary>
        public List<StringVariable> GetVariables()
        {
            // Split the data
            var split = string.IsNullOrEmpty(Type.Separator)
                ? new[] { Data }
                : Data.Split(new string[] { Type.Separator }, Type.Slices.Length, StringSplitOptions.None);

            // If there are less than the required slices, set the missing ones to empty strings
            var toAdd = split.Concat(Enumerable.Repeat(string.Empty, Type.Slices.Length - split.Length));

            return toAdd
                .Zip(Type.Slices, (k, v) => new { k, v })
                .Select(x => new StringVariable(x.k) { Name = x.v })
                .Concat(toAdd
                .Zip(Type.SlicesAlias, (k, v) => new { k, v })
                .Select(x => new StringVariable(x.k) { Name = x.v }))
                .ToList();
        }

        /// <summary>
        /// Checks if the data line respects the data rules.
        /// </summary>
        public bool RespectsRules(IEnumerable<DataRule> rules)
        {
            var variables = GetVariables();

            foreach (var rule in rules)
            {
                var slice = variables.FirstOrDefault(v => v.Name == rule.SliceName);
                
                if (slice == null)
                {
                    throw new ArgumentException($"Invalid slice name ({rule.SliceName}) in a data rule");
                }

                var value = slice.AsString();
                
                if (!rule.IsSatisfied(value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
