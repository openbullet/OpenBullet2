using RuriLib.Attributes;
using RuriLib.Functions.Parsing;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Blocks.Parsing
{
    [BlockCategory("Parsing", "Blocks for extracting data from strings", "#ffd700")]
    public static class Methods
    {
        #region LR
        public static List<string> ParseBetweenStringsRecursive(BotData data, string input, 
            string leftDelim, string rightDelim, bool caseSensitive = true)
        {
            var parsed = LRParser.ParseBetween(input, leftDelim, rightDelim, caseSensitive).ToList();
            data.Logger.LogHeader();
            data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
            data.Logger.Log(parsed, LogColors.Yellow);
            return parsed;
        }

        public static string ParseBetweenStrings(BotData data, string input, 
            string leftDelim, string rightDelim, bool caseSensitive = true)
        {
            var parsed = LRParser.ParseBetween(input, leftDelim, rightDelim, caseSensitive).FirstOrDefault();
            data.Logger.LogHeader();
            data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);
            return parsed;
        }
        #endregion

        #region HTML
        public static List<string> QueryCssSelectorRecursive(BotData data, string htmlPage,
            string cssSelector, string attributeName)
        {
            var parsed = HtmlParser.QueryAttributeAll(htmlPage, cssSelector, attributeName).ToList();
            data.Logger.LogHeader();
            data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
            data.Logger.Log(parsed, LogColors.Yellow);
            return parsed;
        }

        public static string QueryCssSelector(BotData data, string htmlPage, string cssSelector, string attributeName)
        {
            var parsed = HtmlParser.QueryAttributeAll(htmlPage, cssSelector, attributeName).FirstOrDefault();
            data.Logger.LogHeader();
            data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);
            return parsed;
        }
        #endregion

        #region JSON
        public static List<string> QueryJsonTokenRecursive(BotData data, string json, string jToken)
        {
            var parsed = JsonParser.GetValuesByKey(json, jToken).ToList();
            data.Logger.LogHeader();
            data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
            data.Logger.Log(parsed, LogColors.Yellow);
            return parsed;
        }

        public static string QueryJsonToken(BotData data, string json, string jToken)
        {
            var parsed = JsonParser.GetValuesByKey(json, jToken).FirstOrDefault();
            data.Logger.LogHeader();
            data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);
            return parsed;
        }
        #endregion

        #region REGEX
        public static List<string> MatchRegexGroupsRecursive(BotData data, string input,
            string pattern, string outputFormat)
        {
            var parsed = RegexParser.MatchGroupsToString(input, pattern, outputFormat).ToList();
            data.Logger.LogHeader();
            data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
            data.Logger.Log(parsed, LogColors.Yellow);
            return parsed;
        }

        public static string MatchRegexGroups(BotData data, string input, string pattern, string outputFormat)
        {
            var parsed = RegexParser.MatchGroupsToString(input, pattern, outputFormat).FirstOrDefault();
            data.Logger.LogHeader();
            data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);
            return parsed;
        }
        #endregion
    }
}
