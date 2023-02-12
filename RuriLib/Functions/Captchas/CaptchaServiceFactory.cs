using CaptchaSharp;
using CaptchaSharp.Services;
using CaptchaSharp.Services.More;
using RuriLib.Models.Settings;
using System;

namespace RuriLib.Functions.Captchas
{
    public class CaptchaServiceFactory
    {
        /// <summary>
        /// Gets a <see cref="CaptchaService"/> to be used for solving captcha challenges.
        /// </summary>
        public static CaptchaService GetService(CaptchaSettings settings)
        {
            CaptchaService service = settings.CurrentService switch
            {
                CaptchaServiceType.AntiCaptcha => new AntiCaptchaService(settings.AntiCaptchaApiKey),
                CaptchaServiceType.AzCaptcha => new AzCaptchaService(settings.AZCaptchaApiKey),
                CaptchaServiceType.CaptchasIO => new CaptchasIOService(settings.CaptchasDotIoApiKey),
                CaptchaServiceType.CustomTwoCaptcha => new CustomTwoCaptchaService(settings.CustomTwoCaptchaApiKey,
                    GetUri(settings.CustomTwoCaptchaDomain, settings.CustomTwoCaptchaPort),
                    null, settings.CustomTwoCaptchaOverrideHostHeader),
                CaptchaServiceType.CapMonster => new CapMonsterService(string.Empty,
                    GetUri(settings.CapMonsterHost, settings.CapMonsterPort)),
                CaptchaServiceType.DeathByCaptcha => new DeathByCaptchaService(settings.DeathByCaptchaUsername, settings.DeathByCaptchaPassword),
                CaptchaServiceType.DeCaptcher => new DeCaptcherService(settings.DeCaptcherUsername, settings.DeCaptcherPassword),
                CaptchaServiceType.ImageTyperz => new ImageTyperzService(settings.ImageTyperzApiKey),
                CaptchaServiceType.RuCaptcha => new RuCaptchaService(settings.RuCaptchaApiKey),
                CaptchaServiceType.SolveCaptcha => new SolveCaptchaService(settings.SolveCaptchaApiKey),
                CaptchaServiceType.SolveRecaptcha => new SolveRecaptchaService(settings.SolveRecaptchaApiKey),
                CaptchaServiceType.TrueCaptcha => new TrueCaptchaService(settings.TrueCaptchaUsername, settings.TrueCaptchaApiKey),
                CaptchaServiceType.TwoCaptcha => new TwoCaptchaService(settings.TwoCaptchaApiKey),
                CaptchaServiceType.NineKW => new NineKWService(settings.NineKWApiKey),
                CaptchaServiceType.CustomAntiCaptcha => new CustomAntiCaptchaService(settings.CustomAntiCaptchaApiKey,
                    GetUri(settings.CustomAntiCaptchaDomain, settings.CustomAntiCaptchaPort)),
                CaptchaServiceType.AnyCaptcha => new AnyCaptchaService(settings.AnyCaptchaApiKey),
                CaptchaServiceType.CapSolver => new CapSolverService(settings.CapSolverApiKey),
                _ => throw new NotSupportedException(),
            };

            service.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            service.PollingInterval = TimeSpan.FromMilliseconds(settings.PollingIntervalMilliseconds);
            return service;
        }

        private static Uri GetUri(string host, int port)
        {
            // If there is no http(s) then add http by default
            if (!host.StartsWith("http"))
            {
                host = $"http://{host}";
            }

            return new Uri($"{host}:{port}");
        }
    }
}
