using OpenQA.Selenium;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that executes javascript code in the selenium-driven browser.
    /// </summary>
    public class SBlockExecuteJS : BlockBase
    {
        /// <summary>The javascript code.</summary>
        public string JavascriptCode { get; set; } = "alert('henlo');";

        /// <summary>The name of the output variable.</summary>
        public string OutputVariable { get; set; } = "";

        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get; set; } = false;

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
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            var browser = data.TryGetObject<WebDriver>("selenium");

            if (browser == null)
            {
                throw new Exception("Open a browser first!");
            }

            data.Logger.Log("Executing JS code!", LogColors.White);

            var returned = browser.ExecuteScript(ReplaceValues(JavascriptCode, ls));

            if (returned != null)
            {
                try
                {
                    InsertVariable(ls, IsCapture, false, new List<string>() { returned.ToString() }, OutputVariable, "", "", false, true);
                }
                catch
                {
                    throw new Exception($"Failed to convert the returned value to a string");
                }
            }

            data.Logger.Log("... executed!", LogColors.White);

            UpdateSeleniumData(data);
        }
    }
}
