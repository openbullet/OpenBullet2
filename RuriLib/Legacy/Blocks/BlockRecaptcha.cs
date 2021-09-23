using RuriLib.Functions.Captchas;
using RuriLib.LS;
using RuriLib.Models;
using System;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// A block that solves a reCaptcha challenge.
    /// </summary>
    [Obsolete]
    public class BlockRecaptcha : BlockCaptcha
    {
        private string variableName = "";
        /// <summary>The name of the output variable where the challenge solution will be stored.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        private string url = "https://google.com";
        /// <summary>The URL where the reCaptcha challenge appears.</summary>
        public string Url { get { return url; } set { url = value; OnPropertyChanged(); } }

        private string siteKey = "";
        /// <summary>The Google SiteKey found in the page's source code.</summary>
        public string SiteKey { get { return siteKey; } set { siteKey = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates a reCaptcha block.
        /// </summary>
        public BlockRecaptcha()
        {
            Label = "RECAPTCHA";
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
             * RECAPTCHA "URL" "SITEKEY"->VAR "RECAP"
             * */

            Url = LineParser.ParseLiteral(ref input, "URL");
            SiteKey = LineParser.ParseLiteral(ref input, "SITEKEY");

            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == string.Empty)
                return this;

            LineParser.EnsureIdentifier(ref input, "VAR");

            // Parse the variable name
            VariableName = LineParser.ParseLiteral(ref input, "VARIABLE NAME");

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("RECAPTCHA")
                .Literal(Url)
                .Literal(SiteKey)
                .Arrow()
                .Token("VAR")
                .Literal(VariableName);
            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            if(!data.GlobalSettings.Captchas.BypassBalanceCheck)
                base.Process(data);

            data.Log(new LogEntry("WARNING! This block is obsolete and WILL BE REMOVED IN THE FUTURE! Use the SOLVECAPTCHA block!", Colors.Tomato));
            data.Log(new LogEntry("Solving reCaptcha...", Colors.White));

            string recapResponse = "";
            try
            {
                recapResponse = Captchas.GetService(data.GlobalSettings.Captchas)
                    .SolveRecaptchaV2Async(ReplaceValues(siteKey, data), ReplaceValues(url, data)).Result.Response;
            }
            catch
            {
                data.Log(recapResponse == string.Empty ? new LogEntry("Couldn't get a reCaptcha response from the service", Colors.Tomato) : new LogEntry("Succesfully got the response: " + recapResponse, Colors.GreenYellow));
            }

            if (VariableName != string.Empty)
            {
                data.Log(new LogEntry("Response stored in variable: " + variableName, Colors.White));
                data.Variables.Set(new CVar(variableName, recapResponse));
            }
        }
    }
}
