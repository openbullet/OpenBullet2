using RuriLib.LS;
using System;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// A block that navigates to a given URL in a selenium-driven browser.
    /// </summary>
    public class SBlockNavigate : BlockBase
    {
        private string url = "https://example.com";
        /// <summary>The destination URL.</summary>
        public string Url { get { return url; } set { url = value; OnPropertyChanged(); } }

        private int timeout = 60;
        /// <summary>The maximum time to wait for the page to load.</summary>
        public int Timeout { get { return timeout; } set { timeout = value; OnPropertyChanged(); } }

        private bool banOnTimeout = true;
        /// <summary>Whether to set the status to BAN after a timeout.</summary>
        public bool BanOnTimeout { get { return banOnTimeout; } set { banOnTimeout = value; OnPropertyChanged(); } }

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
        public override void Process(BotData data)
        {
            base.Process(data);

            if(data.Driver == null)
            {
                data.Log(new LogEntry("Open a browser first!", Colors.White));
                throw new Exception("Browser not open");
            }
            
            data.Log(new LogEntry("Navigating to "+ReplaceValues(url,data), Colors.White));
            data.Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(timeout);
            try
            {
                data.Driver.Navigate().GoToUrl(ReplaceValues(url, data));
                //data.Driver.Url = ReplaceValues(url, data);
                data.Log(new LogEntry("Navigated!", Colors.White));
            }
            catch (OpenQA.Selenium.WebDriverTimeoutException)
            {
                data.Log(new LogEntry("Timeout on Page Load", Colors.Tomato));

                if (BanOnTimeout)
                {
                    data.Status = BotStatus.BAN;
                }
            }

            data.Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(data.GlobalSettings.Selenium.PageLoadTimeout);
            UpdateSeleniumData(data);
        }
    }
}
