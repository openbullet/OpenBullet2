using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using System.Threading.Tasks;
using RuriLib.Logging;
using RuriLib.Functions.Http.Options;
using RuriLib.Functions.Http;
using RuriLib.Models.Variables;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that solves an image captcha challenge.
    /// </summary>
    public class BlockImageCaptcha : BlockCaptcha
    {
        /// <summary>The URL to download the captcha image from.</summary>
        public string Url { get; set; } = "";

        /// <summary>The name of the variable where the challenge solution will be stored.</summary>
        public string VariableName { get; set; } = "";

        /// <summary>Whether the Url is a base64-encoded captcha image.</summary>
        public bool Base64 { get; set; } = false;

        /// <summary>Whether the captcha image needs to be taken by the last screenshot taken by selenium.</summary>
        public bool SendScreenshot { get; set; } = false;

        /// <summary>The user agent to use in the image download request.</summary>
        public string UserAgent { get; set; } = "";

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
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            var provider = data.Providers.Captcha;

            data.Logger.Log("WARNING! This block is obsolete and WILL BE REMOVED IN THE FUTURE! Use the SOLVECAPTCHA block!", LogColors.Tomato);
            data.Logger.Log("Downloading image...", LogColors.White);

            var localUrl = ReplaceValues(Url, ls);
            var captchaFile = Utils.GetCaptchaPath(data);
            var screenshotFile = Utils.GetScreenshotPath(data);

            if (Base64)
            {
                var bytes = Convert.FromBase64String(localUrl);
                await using var imageFile = new FileStream(captchaFile, FileMode.Create);
                await imageFile.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                await imageFile.FlushAsync().ConfigureAwait(false);
            }
            else if (SendScreenshot && File.Exists(screenshotFile))
            {
                using var image = new Bitmap(screenshotFile);
                image.Save(captchaFile);
            }
            else
            {
                // Try to download the captcha
                try
                {
                    var standardOptions = new StandardHttpRequestOptions
                    {
                        CustomCookies = data.COOKIES,
                        CustomHeaders = new Dictionary<string, string>
                        {
                            { "User-Agent", UserAgent },
                            { "Accept", "*/*" },
                            { "Pragma", "no-cache" },
                            { "Accept-Language", "en-US,en;q=0.8" }
                        },
                        Method = HttpMethod.GET,
                        Url = localUrl,
                        TimeoutMilliseconds = 10000
                    };

                    // Back up the old values
                    var oldAddress = data.ADDRESS;
                    var oldSource = data.SOURCE;
                    var oldResponseCode = data.RESPONSECODE;
                    var oldRawSource = data.RAWSOURCE;

                    // Request the captcha
                    data.Logger.Enabled = false;
                    await RuriLib.Blocks.Requests.Http.Methods.HttpRequestStandard(data, standardOptions);
                    data.Logger.Enabled = true;

                    // Save the image
                    await File.WriteAllBytesAsync(captchaFile, data.RAWSOURCE).ConfigureAwait(false);

                    // Put the old values back
                    data.ADDRESS = oldAddress;
                    data.SOURCE = oldSource;
                    data.RAWSOURCE = oldRawSource;
                    data.RESPONSECODE = oldResponseCode;
                }
                catch (Exception ex)
                {
                    data.Logger.Enabled = true;
                    data.Logger.Log(ex.Message, LogColors.Tomato);
                    throw;
                }
            }

            // Now the captcha is inside the file at path 'captchaFile'

            var response = "";
            var bitmap = new Bitmap(captchaFile);

            try
            {
                var converter = new ImageConverter();
                var bytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));

                var captchaResponse = await provider.SolveImageCaptchaAsync(Convert.ToBase64String(bytes));
                response = captchaResponse.Response;
            }
            catch (Exception ex)
            {
                data.Logger.Log(ex.Message, LogColors.Tomato);
                throw;
            }
            finally
            {
                bitmap.Dispose();
            }

            data.Logger.Log($"Succesfully got the response: {response}", LogColors.GreenYellow);

            if (VariableName != string.Empty)
            {
                GetVariables(data).Set(new StringVariable(response) { Name = VariableName });
                data.Logger.Log($"Response stored in variable: {VariableName}.", LogColors.White);
            }
        }
    }
}
