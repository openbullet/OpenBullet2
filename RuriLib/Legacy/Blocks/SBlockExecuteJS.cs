using RuriLib.LS;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// A block that executes javascript code in the selenium-driven browser.
    /// </summary>
    public class SBlockExecuteJS : BlockBase
    {
        private string javascriptCode = "alert('henlo');";
        /// <summary>The javascript code.</summary>
        public string JavascriptCode { get { return javascriptCode; } set { javascriptCode = value; OnPropertyChanged(); } }

        private string outputVariable = "";
        /// <summary>The name of the output variable.</summary>
        public string OutputVariable { get { return outputVariable; } set { outputVariable = value; OnPropertyChanged(); } }

        private bool isCapture = false;
        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get { return isCapture; } set { isCapture = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates an ExecuteJS block.
        /// </summary>
        public SBlockExecuteJS()
        {
            Label = "EXECUTE JS";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            /*
             * Syntax:
             * EXECUTEJS "SCRIPT"
             * */

            JavascriptCode = LineParser.ParseLiteral(ref input, "SCRIPT");

            // Try to parse the arrow, otherwise just return the block as is with default var name and var / cap choice
            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == string.Empty)
                return this;

            // Parse the VAR / CAP
            try
            {
                var varType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (varType.ToUpper() == "VAR" || varType.ToUpper() == "CAP")
                    IsCapture = varType.ToUpper() == "CAP";
            }
            catch { throw new ArgumentException("Invalid or missing variable type"); }

            // Parse the variable/capture name
            try { OutputVariable = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("EXECUTEJS")
                .Literal(JavascriptCode.Replace("\r\n", " ").Replace("\n", " "));

            if (!writer.CheckDefault(OutputVariable, "OutputVariable"))
            {
                writer
                    .Arrow()
                    .Token(IsCapture ? "CAP" : "VAR")
                    .Literal(OutputVariable);
            }

            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            if (data.Driver == null)
            {
                data.Log(new LogEntry("Open a browser first!", Colors.White));
                throw new Exception("Browser not open");
            }

            data.Log(new LogEntry("Executing JS code!", Colors.White));
            var returned = data.Driver.ExecuteScript(ReplaceValues(javascriptCode, data));

            if (returned != null)
            {
                try
                {
                    InsertVariable(data, isCapture, false, new List<string>() { returned.ToString() }, outputVariable, "", "", false, true);
                }
                catch
                {
                    throw new Exception($"Failed to convert the returned value to a string");
                }
            }

            data.Log(new LogEntry("... executed!", Colors.White));

            UpdateSeleniumData(data);
        }
    }
}
