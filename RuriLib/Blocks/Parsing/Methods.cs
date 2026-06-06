using RuriLib.Attributes;
using RuriLib.Functions.Parsing;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Blocks.Parsing;

/// <summary>
/// Blocks for extracting data from strings.
/// </summary>
[BlockCategory("Parsing", "Blocks for extracting data from strings", "#ffd700")]
public static class Methods
{
    #region LR
    /// <summary>
    /// Parses all values between two delimiters.
    /// </summary>
    public static List<string> ParseBetweenStringsRecursive(BotData data, string input,
        string leftDelim, string rightDelim, bool caseSensitive = true, string prefix = "", string suffix = "",
        bool urlEncodeOutput = false)
    {
        data.Logger.LogHeader();

        var parsed = LRParser.ParseBetween(input, leftDelim, rightDelim, caseSensitive)
            .Select(p => prefix + p + suffix).Select(p => urlEncodeOutput ? Uri.EscapeDataString(p) : p).ToList();

        data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
        data.Logger.Log(parsed, LogColors.Yellow);
        return parsed;
    }

    /// <summary>
    /// Parses the first value between two delimiters.
    /// </summary>
    public static string ParseBetweenStrings(BotData data, string input,
        string leftDelim, string rightDelim, bool caseSensitive = true, string prefix = "", string suffix = "",
        bool urlEncodeOutput = false)
    {
        data.Logger.LogHeader();

        var parsed = LRParser.ParseBetween(input, leftDelim, rightDelim, caseSensitive).FirstOrDefault() ?? string.Empty;
        parsed = prefix + parsed + suffix;

        if (urlEncodeOutput)
        {
            parsed = Uri.EscapeDataString(parsed);
        }

        data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);
        return parsed;
    }
    #endregion

    #region HTML
    /// <summary>
    /// Queries all values matching a CSS selector.
    /// </summary>
    public static List<string> QueryCssSelectorRecursive(BotData data, string htmlPage,
        string cssSelector, string attributeName, string prefix = "", string suffix = "", bool urlEncodeOutput = false)
    {
        data.Logger.LogHeader();

        var parsed = HtmlParser.QueryAttributeAll(htmlPage, cssSelector, attributeName)
            .Select(p => prefix + p + suffix).Select(p => urlEncodeOutput ? Uri.EscapeDataString(p) : p).ToList();

        data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
        data.Logger.Log(parsed, LogColors.Yellow);
        return parsed;
    }

    /// <summary>
    /// Queries the first value matching a CSS selector.
    /// </summary>
    public static string QueryCssSelector(BotData data, string htmlPage, string cssSelector, string attributeName,
        string prefix = "", string suffix = "", bool urlEncodeOutput = false)
    {
        data.Logger.LogHeader();

        var parsed = HtmlParser.QueryAttributeAll(htmlPage, cssSelector, attributeName).FirstOrDefault() ?? string.Empty;
        parsed = prefix + parsed + suffix;

        if (urlEncodeOutput)
        {
            parsed = Uri.EscapeDataString(parsed);
        }

        data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);

        return parsed;
    }
    #endregion

    #region XML
    /// <summary>
    /// Queries all values matching an XPath expression.
    /// </summary>
    public static List<string> QueryXPathRecursive(BotData data, string xmlPage,
        string xPath, string attributeName, string prefix = "", string suffix = "", bool urlEncodeOutput = false)
    {
        data.Logger.LogHeader();

        var parsed = HtmlParser.QueryXPathAll(xmlPage, xPath, attributeName)
            .Select(p => prefix + p + suffix).Select(p => urlEncodeOutput ? Uri.EscapeDataString(p) : p).ToList();

        data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
        data.Logger.Log(parsed, LogColors.Yellow);
        return parsed;
    }

    /// <summary>
    /// Queries the first value matching an XPath expression.
    /// </summary>
    public static string QueryXPath(BotData data, string xmlPage, string xPath, string attributeName,
        string prefix = "", string suffix = "", bool urlEncodeOutput = false)
    {
        data.Logger.LogHeader();

        var parsed = HtmlParser.QueryXPathAll(xmlPage, xPath, attributeName).FirstOrDefault() ?? string.Empty;
        parsed = prefix + parsed + suffix;

        if (urlEncodeOutput)
        {
            parsed = Uri.EscapeDataString(parsed);
        }

        data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);

        return parsed;
    }
    #endregion

    #region JSON
    /// <summary>
    /// Queries all values matching a JSON token.
    /// </summary>
    public static List<string> QueryJsonTokenRecursive(BotData data, string json, string jToken, string prefix = "",
        string suffix = "", bool urlEncodeOutput = false)
    {
        data.Logger.LogHeader();

        var parsed = JsonParser.GetValuesByKey(json, jToken)
            .Select(p => prefix + p + suffix).Select(p => urlEncodeOutput ? Uri.EscapeDataString(p) : p).ToList();

        data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
        data.Logger.Log(parsed, LogColors.Yellow);
        return parsed;
    }

    /// <summary>
    /// Queries the first value matching a JSON token.
    /// </summary>
    public static string QueryJsonToken(BotData data, string json, string jToken, string prefix = "", string suffix = "",
        bool urlEncodeOutput = false)
    {
        data.Logger.LogHeader();

        var parsed = JsonParser.GetValuesByKey(json, jToken).FirstOrDefault() ?? string.Empty;
        parsed = prefix + parsed + suffix;

        if (urlEncodeOutput)
        {
            parsed = Uri.EscapeDataString(parsed);
        }

        data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);
        return parsed;
    }
    #endregion

    #region REGEX
    /// <summary>
    /// Matches all regex groups and formats them as strings.
    /// </summary>
    public static List<string> MatchRegexGroupsRecursive(BotData data, string input,
        string pattern, string outputFormat, bool multiLine, string prefix = "", string suffix = "", bool urlEncodeOutput = false)
    {
        data.Logger.LogHeader();

        var parsed = RegexParser.MatchGroupsToString(input, pattern, outputFormat, multiLine ? RegexOptions.Multiline : RegexOptions.None)
            .Select(p => prefix + p + suffix).Select(p => urlEncodeOutput ? Uri.EscapeDataString(p) : p).ToList();

        data.Logger.Log($"Parsed {parsed.Count} values:", LogColors.Yellow);
        data.Logger.Log(parsed, LogColors.Yellow);
        return parsed;
    }

    /// <summary>
    /// Matches all regex groups and formats them as strings.
    /// </summary>
    /// <remarks>Backwards-compatible overload without the multiline flag.</remarks>
    public static List<string> MatchRegexGroupsRecursive(BotData data, string input,
        string pattern, string outputFormat, string prefix = "", string suffix = "", bool urlEncodeOutput = false)
        => MatchRegexGroupsRecursive(data, input, pattern, outputFormat, false, prefix, suffix, urlEncodeOutput);

    /// <summary>
    /// Matches the first regex group result and formats it as a string.
    /// </summary>
    public static string MatchRegexGroups(BotData data, string input, string pattern, string outputFormat,
        bool multiLine, string prefix = "", string suffix = "", bool urlEncodeOutput = false)
    {
        data.Logger.LogHeader();

        var parsed = RegexParser.MatchGroupsToString(input, pattern, outputFormat, multiLine ? RegexOptions.Multiline : RegexOptions.None).FirstOrDefault() ?? string.Empty;
        parsed = prefix + parsed + suffix;

        if (urlEncodeOutput)
        {
            parsed = Uri.EscapeDataString(parsed);
        }

        data.Logger.Log($"Parsed value: {parsed}", LogColors.Yellow);
        return parsed;
    }

    /// <summary>
    /// Matches the first regex group result and formats it as a string.
    /// </summary>
    /// <remarks>Backwards-compatible overload without the multiline flag.</remarks>
    public static string MatchRegexGroups(BotData data, string input, string pattern, string outputFormat,
        string prefix = "", string suffix = "", bool urlEncodeOutput = false)
        => MatchRegexGroups(data, input, pattern, outputFormat, false, prefix, suffix, urlEncodeOutput);
    #endregion
}
