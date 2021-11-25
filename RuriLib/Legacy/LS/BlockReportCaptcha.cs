using CaptchaSharp.Enums;
using CaptchaSharp.Exceptions;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using System;
using System.Threading.Tasks;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that reports a captcha as incorrectly solved.
    /// </summary>
    public class BlockReportCaptcha : BlockBase
    {
        /// <summary>The type of captcha to report.</summary>
        public CaptchaType Type { get; set; } = CaptchaType.ImageCaptcha;

        /// <summary>The ID of the captcha to report.</summary>
        public string CaptchaId { get; set; } = "<CAPTCHAID>";

        /// <summary>
        /// Creates a ReportCaptcha block.
        /// </summary>
        public BlockReportCaptcha()
        {
            Label = "REPORT CAPTCHA";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            Type = (CaptchaType)LineParser.ParseEnum(ref input, "TYPE", typeof(CaptchaType));
            CaptchaId = LineParser.ParseLiteral(ref input, "CAPTCHA ID");
            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            return new BlockWriter(GetType(), indent, Disabled)
                .Label(Label)
                .Token("REPORTCAPTCHA")
                .Token(Type)
                .Literal(CaptchaId)
                .ToString();
        }

        /// <inheritdoc />
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            string errorMessage;
            var provider = data.Providers.Captcha;

            try
            {
                try
                {
                    var replacedId = ReplaceValues(CaptchaId, ls);
                    await provider.ReportSolution(long.Parse(replacedId), Type);
                    data.Logger.Log($"Captcha reported successfully!", LogColors.GreenYellow);
                    return;
                }
                catch (Exception ex) // This unwraps aggregate exceptions
                {
                    if (ex is AggregateException)
                    {
                        throw ex.InnerException;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (TaskReportException ex)
            {
                errorMessage = $"The captcha report was not accepted! {ex.Message}";
            }
            catch (NotSupportedException ex)
            {
                errorMessage = $"The currently selected service ({provider.ServiceType}) does not support reports! {ex.Message}";
            }
            catch (Exception ex)
            {
                errorMessage = $"An error occurred! {ex.Message}";
            }


            if (!string.IsNullOrEmpty(errorMessage))
            {
                data.Logger.Log(errorMessage, LogColors.Tomato);
            }
        }
    }
}
