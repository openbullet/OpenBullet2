using AngleSharp.Html.Parser;
using Extreme.Net;
using Newtonsoft.Json.Linq;
using RuriLib.LS;
using RuriLib.Utils.Parsing;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Linq;

namespace RuriLib
{
    /// <summary>
    /// The allowed parsing algorithms.
    /// </summary>
    public enum ParseType
    {
        /// <summary>Algorithm that parses text between two strings.</summary>
        LR,

        /// <summary>Algorithm that parses a given attribute from an HTML element identified by a CSS Selector.</summary>
        CSS,

        /// <summary>Algorithm that parses values inside a json object.</summary>
        JSON,

        /// <summary>Algorithm that parses a given attribute from an HTML element identified by xpath.</summary>
        XPATH,

        /// <summary>Algorithm that extracts the text inside matched regex groups.</summary>
        REGEX
    }

    /// <summary>
    /// A block that parses data from a string.
    /// </summary>
    public class BlockParse : BlockBase
    {
        private string parseTarget = "<SOURCE>";
        /// <summary>The string to parse data from.</summary>
        public string ParseTarget { get { return parseTarget; } set { parseTarget = value; OnPropertyChanged(); } }

        private string variableName = "";
        /// <summary>The name of the output variable where the parsed text will be stored.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        private bool isCapture = false;
        /// <summary>Whether the output variable should be marked as Capture.</summary>
        public bool IsCapture { get { return isCapture; } set { isCapture = value; OnPropertyChanged(); } }

        private string prefix = "";
        /// <summary>The string to add to the beginning of the parsed data.</summary>
        public string Prefix { get { return prefix; } set { prefix = value; OnPropertyChanged(); } }

        private string suffix = "";
        /// <summary>The string to add to the end of the parsed data.</summary>
        public string Suffix { get { return suffix; } set { suffix = value; OnPropertyChanged(); } }

        private bool recursive = false;
        /// <summary>Whether to parse multiple values that match the criteria or just the first one.</summary>
        public bool Recursive { get { return recursive; } set { recursive = value; OnPropertyChanged(); } }

        private bool dotMatches = false;
        /// <summary>Whether Regex . matches over multiple lines.</summary>
        public bool DotMatches { get { return dotMatches; } set { dotMatches = value; OnPropertyChanged(); } }

        private bool caseSensitive = true;
        /// <summary>Whether Regex matches are case sensitive.</summary>
        public bool CaseSensitive { get { return caseSensitive; } set { caseSensitive = value; OnPropertyChanged(); } }

        private bool encodeOutput = false;
        /// <summary>Whether to URL encode the parsed text.</summary>
        public bool EncodeOutput { get { return encodeOutput; } set { encodeOutput = value; OnPropertyChanged(); } }

        private bool createEmpty = true;
        /// <summary>Whether to create the variable with an empty value if the parsing was not successful.</summary>
        public bool CreateEmpty { get { return createEmpty; } set { createEmpty = value; OnPropertyChanged(); } }

        private ParseType type = ParseType.LR;
        /// <summary>The parsing algorithm being used.</summary>
        public ParseType Type { get { return type; } set { type = value; OnPropertyChanged(); } }

        #region LR
        private string leftString = "";
        /// <summary>The string to the immediate left of the text to parse. An empty string means the start of the input.</summary>
        public string LeftString { get { return leftString; } set { leftString = value; OnPropertyChanged(); } }

        private string rightString = "";
        /// <summary>The string to the immediate right of the text to parse. An empty string means the end of the input.</summary>
        public string RightString { get { return rightString; } set { rightString = value; OnPropertyChanged(); } }

        private bool useRegexLR = false;
        /// <summary>Whether to use a regex pattern to match a text between two strings instead of the standard algorithm.</summary>
        public bool UseRegexLR { get { return useRegexLR; } set { useRegexLR = value; OnPropertyChanged(); } }
        #endregion

        #region CSS
        private string cssSelector = "";
        /// <summary>The CSS selector that addresses the desired element in the HTML page.</summary>
        public string CssSelector { get { return cssSelector; } set { cssSelector = value; OnPropertyChanged(); } }

        private string attributeName = "";
        /// <summary>The name of the attribute from which to parse the value.</summary>
        public string AttributeName { get { return attributeName; } set { attributeName = value; OnPropertyChanged(); } }

        private int cssElementIndex = 0;
        /// <summary>The index of the desired element when the selector matches multiple elements.</summary>
        public int CssElementIndex { get { return cssElementIndex; } set { cssElementIndex = value; OnPropertyChanged(); } }
        #endregion

        #region JSON
        private string jsonField = "";
        /// <summary>The name of the json field for which we want to retrieve the value.</summary>
        public string JsonField { get { return jsonField; } set { jsonField = value; OnPropertyChanged(); } }

        private bool jTokenParsing = false;
        /// <summary>Whether to parse the json object using jtoken paths.</summary>
        public bool JTokenParsing { get { return jTokenParsing; } set { jTokenParsing = value; OnPropertyChanged(); } }
        #endregion

        #region REGEX
        private string regexString = "";
        /// <summary>The regex pattern that matches parts of the text inside groups.</summary>
        public string RegexString { get { return regexString; } set { regexString = value; OnPropertyChanged(); } }

        private string regexOutput = "";
        /// <summary>The way the content of the matched groups should be formatted. [0] will be replaced with the full match, [1] with the first group etc.</summary>
        public string RegexOutput { get { return regexOutput; } set { regexOutput = value; OnPropertyChanged(); } }
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
        public override void Process(BotData data)
        {
            base.Process(data);

            var original = ReplaceValues(parseTarget, data);
            var list = new List<string>();

            // Parse the value
            switch (Type)
            {
                case ParseType.LR:
                    list = Parse.LR(original, ReplaceValues(leftString, data), ReplaceValues(rightString, data), recursive, useRegexLR).ToList();
                    break;

                case ParseType.CSS:
                    list = Parse.CSS(original, ReplaceValues(cssSelector, data), ReplaceValues(attributeName, data), cssElementIndex, recursive).ToList();
                    break;

                case ParseType.JSON:
                    list = Parse.JSON(original, ReplaceValues(jsonField, data), recursive, jTokenParsing).ToList();
                    break;

                case ParseType.XPATH:
                    throw new NotImplementedException("XPATH parsing is not implemented yet");

                case ParseType.REGEX:
                    RegexOptions regexOptions = new RegexOptions();
                    if (dotMatches)
                        regexOptions |= RegexOptions.Singleline;
                    if (caseSensitive == false)
                        regexOptions |= RegexOptions.IgnoreCase;
                    list = Parse.REGEX(original, ReplaceValues(regexString, data), ReplaceValues(regexOutput, data), recursive, regexOptions).ToList();
                    break;
            }

            InsertVariable(data, isCapture, recursive, list, variableName, prefix, suffix, encodeOutput, createEmpty);
        }
    }
}
