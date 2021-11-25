using OpenQA.Selenium;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using System;
using System.Threading.Tasks;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that navigates to a given URL in a selenium-driven browser.
    /// </summary>
    public class SBlockNavigate : BlockBase
    {
        /// <summary>The destination URL.</summary>
        public string Url { get; set; } = "https://example.com";

        /// <summary>The maximum time to wait for the page to load.</summary>
        public int Timeout { get; set; } = 60;

        /// <summary>Whether to set the status to BAN after a timeout.</summary>
        public bool BanOnTimeout { get; set; } = true;

        /// <summary>
        /// Creates a Navigate block.
        /// </summary>
        public SBlockNavigate()
        {
            Label = "NAVIGATE";
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
             * NAVIGATE "URL" [TIMEOUT] [BanOnTimeout?]
             * */

            Url = LineParser.ParseLiteral(ref input, "URL");
            if (LineParser.Lookahead(ref input) == TokenType.Integer)
                Timeout = LineParser.ParseInt(ref input, "TIMEOUT");
            if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                LineParser.SetBool(ref input, this);

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("NAVIGATE")
                .Literal(Url)
                .Integer(Timeout, "Timeout")
                .Boolean(BanOnTimeout, "BanOnTimeout");
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

            var replacedUrl = ReplaceValues(Url, ls);
            data.Logger.Log($"Navigating to {replacedUrl}", LogColors.White);
            browser.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(Timeout);

            try
            {
                browser.Navigate().GoToUrl(replacedUrl);
                data.Logger.Log("Navigated!", LogColors.White);
            }
            catch (WebDriverTimeoutException)
            {
                data.Logger.Log("Timeout on Page Load", LogColors.Tomato);

                if (BanOnTimeout)
                {
                    data.STATUS = "BAN";
                }
            }

            UpdateSeleniumData(data);
        }
    }
}
