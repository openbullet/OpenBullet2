using CaptchaSharp.Enums;
using CaptchaSharp.Exceptions;
using RuriLib.Functions.Captchas;
using RuriLib.LS;
using System;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// A block that reports a captcha as incorrectly solved.
    /// </summary>
    public class BlockReportCaptcha : BlockBase
    {
        private CaptchaType type = CaptchaType.ImageCaptcha;
        /// <summary>The type of captcha to report.</summary>
        public CaptchaType Type { get { return type; } set { type = value; OnPropertyChanged(); } }

        private string captchaId = "<CAPTCHAID>";
        /// <summary>The ID of the captcha to report.</summary>
        public string CaptchaId { get { return captchaId; } set { captchaId = value; OnPropertyChanged(); } }

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
        public override void Process(BotData data)
        {
            base.Process(data);

            string errorMessage;
            var service = Captchas.GetService(data.GlobalSettings.Captchas);

            try
            {
                try
                {
                    var replacedId = ReplaceValues(CaptchaId, data);
                    service.ReportSolution(long.Parse(replacedId), Type).Wait();
                    data.Log(new LogEntry($"Captcha reported successfully!", Colors.GreenYellow));
                    return;
                }
                catch (Exception ex) // This unwraps aggregate exceptions
                {
                    if (ex is AggregateException) throw ex.InnerException;
                    else throw;
                }
            }
            catch (TaskReportException ex) { errorMessage = $"The captcha report was not accepted! {ex.Message}"; }
            catch (NotSupportedException ex) { errorMessage = 
                    $"The currently selected service ({data.GlobalSettings.Captchas.CurrentService}) does not support reports! {ex.Message}"; }
            catch (Exception ex) { errorMessage = $"An error occurred! {ex.Message}"; }

            data.Log(new LogEntry(errorMessage, Colors.Tomato));
        }
    }
}
