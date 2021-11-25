using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using RuriLib.Models.Variables;
using System;
using System.Threading.Tasks;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that solves a reCaptcha challenge.
    /// </summary>
    public class BlockRecaptcha : BlockCaptcha
    {
        /// <summary>The name of the output variable where the challenge solution will be stored.</summary>
        public string VariableName { get; set; } = "";

        /// <summary>The URL where the reCaptcha challenge appears.</summary>
        public string Url { get; set; } = "https://google.com";

        /// <summary>The Google SiteKey found in the page's source code.</summary>
        public string SiteKey { get; set; } = "";

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
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            var provider = data.Providers.Captcha;

            data.Logger.Log("WARNING! This block is obsolete and WILL BE REMOVED IN THE FUTURE! Use the SOLVECAPTCHA block!", LogColors.Tomato);
            data.Logger.Log("Solving reCaptcha...", LogColors.White);

            var recapResponse = "";

            try
            {
                var response = await provider.SolveRecaptchaV2Async(ReplaceValues(SiteKey, ls), ReplaceValues(Url, ls));
                recapResponse = response.Response;
            }
            catch (Exception ex)
            {
                data.Logger.Log(ex.Message, LogColors.Tomato);
                throw;
            }

            data.Logger.Log($"Succesfully got the response: {recapResponse}", LogColors.GreenYellow);

            if (VariableName != string.Empty)
            {
                GetVariables(data).Set(new StringVariable(recapResponse) { Name = VariableName });
                data.Logger.Log($"Response stored in variable: {VariableName}.", LogColors.White);
            }
        }
    }
}
