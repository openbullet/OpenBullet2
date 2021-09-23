using RuriLib.LS;
using RuriLib.Models;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media;
using RuriLib.Functions.Download;
using System.Collections.Generic;
using RuriLib.Functions.Captchas;

namespace RuriLib
{
    /// <summary>
    /// A block that solves an image captcha challenge.
    /// </summary>
    [Obsolete]
    public class BlockImageCaptcha : BlockCaptcha
    {
        private string url = "";
        /// <summary>The URL to download the captcha image from.</summary>
        public string Url { get { return url; } set { url = value; OnPropertyChanged(); } }

        private string variableName = "";
        /// <summary>The name of the variable where the challenge solution will be stored.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        private bool base64 = false;
        /// <summary>Whether the Url is a base64-encoded captcha image.</summary>
        public bool Base64 { get { return base64; } set { base64 = value; OnPropertyChanged(); } }

        private bool sendScreenshot = false;
        /// <summary>Whether the captcha image needs to be taken by the last screenshot taken by selenium.</summary>
        public bool SendScreenshot { get { return sendScreenshot; } set { sendScreenshot = value; OnPropertyChanged(); } }

        private string userAgent = "";
        /// <summary>The user agent to use in the image download request.</summary>
        public string UserAgent { get { return userAgent; } set { userAgent = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates an Image Captcha block.
        /// </summary>
        public BlockImageCaptcha()
        {
            Label = "CAPTCHA";
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
             * CAPTCHA "URL" [BASE64? USESCREEN?] -> VAR "CAP"
             * */

            Url = LineParser.ParseLiteral(ref input, "URL");

            if (LineParser.Lookahead(ref input) == TokenType.Literal)
            {
                UserAgent = LineParser.ParseLiteral(ref input, "UserAgent");
            }

            while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                LineParser.SetBool(ref input, this);

            LineParser.EnsureIdentifier(ref input, "->");
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
                .Token("CAPTCHA")
                .Literal(Url);

            if (UserAgent != string.Empty) writer.Literal(UserAgent);

            writer.Boolean(Base64, "Base64")
                .Boolean(SendScreenshot, "SendScreenshot")
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

            var localUrl = ReplaceValues(url, data);

            data.Log(new LogEntry("WARNING! This block is obsolete and WILL BE REMOVED IN THE FUTURE! Use the SOLVECAPTCHA block!", Colors.Tomato));
            data.Log(new LogEntry("Downloading image...", Colors.White));

            // Download captcha
            var captchaFile = string.Format("Captchas/captcha{0}.jpg", data.BotNumber);
            if (base64)
            {
                var bytes = Convert.FromBase64String(localUrl);
                using (var imageFile = new FileStream(captchaFile, FileMode.Create))
                {
                    imageFile.Write(bytes, 0, bytes.Length);
                    imageFile.Flush();
                }
            }
            else if (sendScreenshot && data.Screenshots.Count > 0)
            {
                Bitmap image = new Bitmap(data.Screenshots.Last());
                image.Save(captchaFile);
            }
            else
            {
                try
                {
                    Download.RemoteFile(captchaFile, localUrl,
                        data.UseProxies, data.Proxy, data.Cookies, out Dictionary<string, string> newCookies,
                        data.GlobalSettings.General.RequestTimeout * 1000, ReplaceValues(UserAgent, data));
                    data.Cookies = newCookies;
                }
                catch (Exception ex) { data.Log(new LogEntry(ex.Message, Colors.Tomato)); throw; }
            }

            string response = "";
            
            var bitmap = new Bitmap(captchaFile);
            try
            {
                var converter = new ImageConverter();
                var bytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));

                response = Captchas.GetService(data.GlobalSettings.Captchas)
                    .SolveImageCaptchaAsync(Convert.ToBase64String(bytes)).Result.Response;
            }
            catch(Exception ex) { data.Log(new LogEntry(ex.Message, Colors.Tomato)); throw; }
            finally { bitmap.Dispose(); }

            data.Log(response == string.Empty ? new LogEntry("Couldn't get a response from the service", Colors.Tomato) : new LogEntry("Succesfully got the response: " + response, Colors.GreenYellow));

            if (VariableName != string.Empty)
            {
                data.Log(new LogEntry("Response stored in variable: " + variableName, Colors.White));
                data.Variables.Set(new CVar(variableName, response));
            }
        }
    }
}
