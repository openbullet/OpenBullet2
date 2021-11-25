using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using System.Threading.Tasks;
using RuriLib.Functions.Parsing;
using Newtonsoft.Json.Linq;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that parses data from a string.
    /// </summary>
    public class BlockParse : BlockBase
    {
        /// <summary>The string to parse data from.</summary>
        public string ParseTarget { get; set; } = "<SOURCE>";

        /// <summary>The name of the output variable where the parsed text will be stored.</summary>
        public string VariableName { get; set; } = "";

        /// <summary>Whether the output variable should be marked as Capture.</summary>
        public bool IsCapture { get; set; } = false;

        /// <summary>The string to add to the beginning of the parsed data.</summary>
        public string Prefix { get; set; } = "";

        /// <summary>The string to add to the end of the parsed data.</summary>
        public string Suffix { get; set; } = "";

        /// <summary>Whether to parse multiple values that match the criteria or just the first one.</summary>
        public bool Recursive { get; set; } = false;

        /// <summary>Whether Regex . matches over multiple lines.</summary>
        public bool DotMatches { get; set; } = false;

        /// <summary>Whether Regex matches are case sensitive.</summary>
        public bool CaseSensitive { get; set; } = true;

        /// <summary>Whether to URL encode the parsed text.</summary>
        public bool EncodeOutput { get; set; } = false;

        /// <summary>Whether to create the variable with an empty value if the parsing was not successful.</summary>
        public bool CreateEmpty { get; set; } = true;

        /// <summary>The parsing algorithm being used.</summary>
        public ParseType Type { get; set; } = ParseType.LR;

        #region LR
        /// <summary>The string to the immediate left of the text to parse. An empty string means the start of the input.</summary>
        public string LeftString { get; set; } = "";

        /// <summary>The string to the immediate right of the text to parse. An empty string means the end of the input.</summary>
        public string RightString { get; set; } = "";

        /// <summary>Whether to use a regex pattern to match a text between two strings instead of the standard algorithm.</summary>
        public bool UseRegexLR { get; set; } = false;
        #endregion

        #region CSS
        /// <summary>The CSS selector that addresses the desired element in the HTML page.</summary>
        public string CssSelector { get; set; } = "";

        /// <summary>The name of the attribute from which to parse the value.</summary>
        public string AttributeName { get; set; } = "";

        /// <summary>The index of the desired element when the selector matches multiple elements.</summary>
        public int CssElementIndex { get; set; } = 0;
        #endregion

        #region JSON
        /// <summary>The name of the json field for which we want to retrieve the value.</summary>
        public string JsonField { get; set; } = "";

        /// <summary>Whether to parse the json object using jtoken paths.</summary>
        public bool JTokenParsing { get; set; } = false;
        #endregion

        #region REGEX
        /// <summary>The regex pattern that matches parts of the text inside groups.</summary>
        public string RegexString { get; set; } = "";

        /// <summary>The way the content of the matched groups should be formatted. [0] will be replaced with the full match, [1] with the first group etc.</summary>
        public string RegexOutput { get; set; } = "";
        #endregion

        /// <summary>
        /// Creates a Parse block.
        /// </summary>
        public BlockParse()
        {
            Label = "PARSE";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            ParseTarget = LineParser.ParseLiteral(ref input, "TARGET");

            Type = (ParseType)LineParser.ParseEnum(ref input, "TYPE", typeof(ParseType));

            switch (Type)
            {
                case ParseType.LR:
                    // PARSE "<SOURCE>" LR "L" "R" RECURSIVE? -> VAR/CAP "ABC"
                    LeftString = LineParser.ParseLiteral(ref input, "LEFT STRING");
                    RightString = LineParser.ParseLiteral(ref input, "RIGHT STRING");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case ParseType.CSS:
                    // PARSE "<SOURCE>" CSS "Selector" "Attribute" Index RECURSIVE? ->
                    CssSelector = LineParser.ParseLiteral(ref input, "SELECTOR");
                    AttributeName = LineParser.ParseLiteral(ref input, "ATTRIBUTE");
                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    else if (LineParser.Lookahead(ref input) == TokenType.Integer)
                        CssElementIndex = LineParser.ParseInt(ref input, "INDEX");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case ParseType.JSON:
                    // PARSE "<SOURCE>" JSON "Field" ->
                    JsonField = LineParser.ParseLiteral(ref input, "FIELD");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case ParseType.REGEX:
                    // PARSE "<SOURCE>" REGEX "Pattern" "Output" RECURSIVE? -> 
                    RegexString = LineParser.ParseLiteral(ref input, "PATTERN");
                    RegexOutput = LineParser.ParseLiteral(ref input, "OUTPUT");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;
            }

            // Parse the arrow
            LineParser.ParseToken(ref input, TokenType.Arrow, true);

            // Parse the VAR / CAP
            try
            {
                var varType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (varType.ToUpper() == "VAR" || varType.ToUpper() == "CAP")
                    IsCapture = varType.ToUpper() == "CAP";
            }
            catch { throw new ArgumentException("Invalid or missing variable type"); }

            // Parse the variable/capture name
            try { VariableName = LineParser.ParseLiteral(ref input, "NAME"); }
            catch { throw new ArgumentException("Variable name not specified"); }

            // Parse the prefix and suffix
            try
            {
                Prefix = LineParser.ParseLiteral(ref input, "PREFIX");
                Suffix = LineParser.ParseLiteral(ref input, "SUFFIX");
            }
            catch { }

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("PARSE")
                .Literal(ParseTarget)
                .Token(Type);

            switch (Type)
            {
                case ParseType.LR:
                    writer
                        .Literal(LeftString)
                        .Literal(RightString)
                        .Boolean(Recursive, "Recursive")
                        .Boolean(EncodeOutput, "EncodeOutput")
                        .Boolean(CreateEmpty, "CreateEmpty")
                        .Boolean(UseRegexLR, "UseRegexLR");
                    break;

                case ParseType.CSS:
                    writer
                        .Literal(CssSelector)
                        .Literal(AttributeName);
                    if (Recursive) writer.Boolean(Recursive, "Recursive");
                    else writer.Integer(CssElementIndex, "CssElementIndex");

                    writer
                        .Boolean(EncodeOutput, "EncodeOutput")
                        .Boolean(CreateEmpty, "CreateEmpty");
                    break;

                case ParseType.JSON:
                    writer
                        .Literal(JsonField)
                        .Boolean(JTokenParsing, "JTokenParsing")
                        .Boolean(Recursive, "Recursive")
                        .Boolean(EncodeOutput, "EncodeOutput")
                        .Boolean(CreateEmpty, "CreateEmpty");
                    break;

                case ParseType.REGEX:
                    writer
                        .Literal(RegexString)
                        .Literal(RegexOutput)
                        .Boolean(Recursive, "Recursive")
                        .Boolean(EncodeOutput, "EncodeOutput")
                        .Boolean(CreateEmpty, "CreateEmpty")
                        .Boolean(DotMatches, "DotMatches")
                        .Boolean(CaseSensitive, "CaseSensitive");
                    break;
            }

            writer
                .Arrow()
                .Token(IsCapture ? "CAP" : "VAR")
                .Literal(VariableName);

            if (!writer.CheckDefault(Prefix, "Prefix") || !writer.CheckDefault(Suffix, "Suffix"))
                    writer.Literal(Prefix).Literal(Suffix);

            return writer.ToString();
        }

        /// <inheritdoc />
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            var original = ReplaceValues(ParseTarget, ls);
            var list = Type switch
            {
                ParseType.LR => ParseLR(original, ReplaceValues(LeftString, ls), ReplaceValues(RightString, ls), UseRegexLR),
                ParseType.CSS => ParseCSS(original, ReplaceValues(CssSelector, ls), ReplaceValues(AttributeName, ls)),
                ParseType.JSON => ParseJSON(original, ReplaceValues(JsonField, ls), JTokenParsing),
                ParseType.REGEX => ParseREGEX(original, ReplaceValues(RegexString, ls), ReplaceValues(RegexOutput, ls), DotMatches, CaseSensitive),
                _ => throw new NotImplementedException()
            };

            if (!Recursive && Type == ParseType.CSS)
            {
                list = new List<string> { list[CssElementIndex] };
            }

            InsertVariable(ls, IsCapture, Recursive, list, VariableName, Prefix, Suffix, EncodeOutput, CreateEmpty);
        }

        private static List<string> ParseLR(string original, string left, string right, bool useRegex)
        {
            List<string> list;

            if (!useRegex)
            {
                list = LRParser.ParseBetween(original, left, right).ToList();
            }
            else
            {
                list = new();
                var pattern = BuildLRPattern(left, right);
                
                foreach (Match match in Regex.Matches(original, pattern))
                {
                    list.Add(match.Value);
                }
            }

            return list;
        }

        private static List<string> ParseCSS(string original, string selector, string attributeName)
            => HtmlParser.QueryAttributeAll(original, selector, attributeName).ToList();

        private static List<string> ParseJSON(string original, string fieldName, bool jTokenParsing)
        {
            if (jTokenParsing)
            {
                return JsonParser.GetValuesByKey(original, fieldName).ToList();
            }

            var list = new List<string>();
            var jsonlist = new List<KeyValuePair<string, string>>();
            ParseJSON("", original, jsonlist);

            foreach (var j in jsonlist)
            {
                if (j.Key == fieldName)
                {
                    list.Add(j.Value);
                }
            }

            return list;
        }

        private static List<string> ParseREGEX(string original, string pattern, string format, bool multiLine, bool caseSensitive)
        {
            var regexOptions = new RegexOptions();

            if (multiLine)
            {
                regexOptions |= RegexOptions.Singleline;
            }
            if (!caseSensitive)
            {
                regexOptions |= RegexOptions.IgnoreCase;
            }

            return RegexParser.MatchGroupsToString(original, pattern, format, regexOptions).ToList();
        }

        private static string BuildLRPattern(string ls, string rs)
        {
            var left = string.IsNullOrEmpty(ls) ? "^" : Regex.Escape(ls); // Empty LEFT = start of the line
            var right = string.IsNullOrEmpty(rs) ? "$" : Regex.Escape(rs); // Empty RIGHT = end of the line
            return "(?<=" + left + ").+?(?=" + right + ")";
        }

        private static void ParseJSON(string A, string B, List<KeyValuePair<string, string>> jsonlist)
        {
            jsonlist.Add(new KeyValuePair<string, string>(A, B));

            if (B.StartsWith("["))
            {
                JArray arr;

                try
                {
                    arr = JArray.Parse(B);
                }
                catch
                {
                    return;
                }

                foreach (var i in arr.Children())
                {
                    ParseJSON("", i.ToString(), jsonlist);
                }
            }

            if (B.Contains("{"))
            {
                JObject obj;

                try
                {
                    obj = JObject.Parse(B);
                }
                catch
                {
                    return;
                }

                foreach (var o in obj)
                {
                    ParseJSON(o.Key, o.Value.ToString(), jsonlist);
                }
            }
        }
    }

    public enum ParseType
    {
        LR,
        CSS,
        JSON,
        XPATH,
        REGEX
    }
}
