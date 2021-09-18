using RuriLib.Attributes;
using RuriLib.Functions.Parsing;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Blocks.Parsing
{
    [BlockCategory("Parsing", "Blocks for extracting data from strings", "#ffd700")]
    public static class Methods
    {
        #region LR
        public static List<string> ParseBetweenStringsRecursive(BotData data, string input, 
            string leftDelim, string rightDelim, bool caseSensitive = true, string prefix = "", string suffix = "",
            bool urlEncodeOutput = false)
        {
            var parsed = LRParser.ParseBetween(input, leftDelim, rightDelim, caseSensitive)
                .Select(p => prefix + p + suffix).Select(p => urlEncodeOutput ? Uri.UnescapeDataString(p) : p).ToList();

            data.Logger.LogHeader();
            data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
            data.Logger.Log(parsed, LogColors.Yellow);
            return parsed;
        }

        public static string ParseBetweenStrings(BotData data, string input, 
            string leftDelim, string rightDelim, bool caseSensitive = true, string prefix = "", string suffix = "",
            bool urlEncodeOutput = false)
        {
            var parsed = LRParser.ParseBetween(input, leftDelim, rightDelim, caseSensitive).FirstOrDefault() ?? string.Empty;
            parsed = prefix + parsed + suffix;

            if (urlEncodeOutput)
            {
                parsed = Uri.EscapeDataString(parsed);
            }

            data.Logger.LogHeader();
            data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);
            return parsed;
        }
        #endregion

        #region HTML
        public static List<string> QueryCssSelectorRecursive(BotData data, string htmlPage,
            string cssSelector, string attributeName, string prefix = "", string suffix = "", bool urlEncodeOutput = false)
        {
            var parsed = HtmlParser.QueryAttributeAll(htmlPage, cssSelector, attributeName)
                .Select(p => prefix + p + suffix).Select(p => urlEncodeOutput ? Uri.UnescapeDataString(p) : p).ToList();

            data.Logger.LogHeader();
            data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
            data.Logger.Log(parsed, LogColors.Yellow);
            return parsed;
        }

        public static string QueryCssSelector(BotData data, string htmlPage, string cssSelector, string attributeName,
            string prefix = "", string suffix = "", bool urlEncodeOutput = false)
        {
            var parsed = HtmlParser.QueryAttributeAll(htmlPage, cssSelector, attributeName).FirstOrDefault() ?? string.Empty;
            parsed = prefix + parsed + suffix;

            if (urlEncodeOutput)
            {
                parsed = Uri.EscapeDataString(parsed);
            }

            data.Logger.LogHeader();
            data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);

            return parsed;
        }
        #endregion

        #region XML
        public static List<string> QueryXPathRecursive(BotData data, string xmlPage,
            string xPath, string attributeName, string prefix = "", string suffix = "", bool urlEncodeOutput = false)
        {
            var parsed = HtmlParser.QueryXPathAll(xmlPage, xPath, attributeName)
                .Select(p => prefix + p + suffix).Select(p => urlEncodeOutput ? Uri.UnescapeDataString(p) : p).ToList();

            data.Logger.LogHeader();
            data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
            data.Logger.Log(parsed, LogColors.Yellow);
            return parsed;
        }

        public static string QueryXPath(BotData data, string xmlPage, string xPath, string attributeName,
            string prefix = "", string suffix = "", bool urlEncodeOutput = false)
        {
            var parsed = HtmlParser.QueryXPathAll(xmlPage, xPath, attributeName).FirstOrDefault() ?? string.Empty;
            parsed = prefix + parsed + suffix;

            if (urlEncodeOutput)
            {
                parsed = Uri.EscapeDataString(parsed);
            }

            data.Logger.LogHeader();
            data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);

            return parsed;
        }
        #endregion

        #region JSON
        public static List<string> QueryJsonTokenRecursive(BotData data, string json, string jToken, string prefix = "",
            string suffix = "", bool urlEncodeOutput = false)
        {
            var parsed = JsonParser.GetValuesByKey(json, jToken)
                .Select(p => prefix + p + suffix).Select(p => urlEncodeOutput ? Uri.UnescapeDataString(p) : p).ToList();

            data.Logger.LogHeader();
            data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
            data.Logger.Log(parsed, LogColors.Yellow);
            return parsed;
        }

        public static string QueryJsonToken(BotData data, string json, string jToken, string prefix = "", string suffix = "",
            bool urlEncodeOutput = false)
        {
            var parsed = JsonParser.GetValuesByKey(json, jToken).FirstOrDefault() ?? string.Empty;
            parsed = prefix + parsed + suffix;

            if (urlEncodeOutput)
            {
                parsed = Uri.EscapeDataString(parsed);
            }

            data.Logger.LogHeader();
            data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);
            return parsed;
        }
        #endregion

        #region REGEX
        public static List<string> MatchRegexGroupsRecursive(BotData data, string input,
            string pattern, string outputFormat, bool multiLine, string prefix = "", string suffix = "", bool urlEncodeOutput = false)
        {
            var parsed = RegexParser.MatchGroupsToString(input, pattern, outputFormat, multiLine ? RegexOptions.Multiline : RegexOptions.None)
                .Select(p => prefix + p + suffix).Select(p => urlEncodeOutput ? Uri.UnescapeDataString(p) : p).ToList();

            data.Logger.LogHeader();
            data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
            data.Logger.Log(parsed, LogColors.Yellow);
            return parsed;
        }

        // Old signature (without multiLine) for backwards compatibility
        public static List<string> MatchRegexGroupsRecursive(BotData data, string input,
            string pattern, string outputFormat, string prefix = "", string suffix = "", bool urlEncodeOutput = false)
            => MatchRegexGroupsRecursive(data, input, pattern, outputFormat, false, prefix, suffix, urlEncodeOutput);

        public static string MatchRegexGroups(BotData data, string input, string pattern, string outputFormat,
            bool multiLine, string prefix = "", string suffix = "", bool urlEncodeOutput = false)
        {
            var parsed = RegexParser.MatchGroupsToString(input, pattern, outputFormat, multiLine ? RegexOptions.Multiline : RegexOptions.None).FirstOrDefault() ?? string.Empty;
            parsed = prefix + parsed + suffix;

            if (urlEncodeOutput)
            {
                parsed = Uri.EscapeDataString(parsed);
            }

            data.Logger.LogHeader();
            data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);
            return parsed;
        }

        // Old signature (without multiLine) for backwards compatibility
        public static string MatchRegexGroups(BotData data, string input, string pattern, string outputFormat,
            string prefix = "", string suffix = "", bool urlEncodeOutput = false)
            => MatchRegexGroups(data, input, pattern, outputFormat, false, prefix, suffix, urlEncodeOutput);
        #endregion
    }
}
