using CaptchaSharp.Services;
using RuriLib.Models.Settings;
using System;

namespace RuriLib.Functions.Captchas;

public class CaptchaServiceFactory
{
    /// <summary>
    /// Gets a <see cref="CaptchaService"/> to be used for solving captcha challenges.
    /// </summary>
    public static CaptchaService GetService(CaptchaSettings settings)
    {
        CaptchaService service = settings.CurrentService switch
        {
            CaptchaServiceType.TwoCaptcha => new TwoCaptchaService(settings.TwoCaptchaApiKey),
            CaptchaServiceType.AntiCaptcha => new AntiCaptchaService(settings.AntiCaptchaApiKey),
            CaptchaServiceType.CustomTwoCaptcha => new CustomTwoCaptchaService(settings.CustomTwoCaptchaApiKey,
                GetUri(settings.CustomTwoCaptchaDomain, settings.CustomTwoCaptchaPort),
                null, settings.CustomTwoCaptchaOverrideHostHeader),
            CaptchaServiceType.DeathByCaptcha => new DeathByCaptchaService(settings.DeathByCaptchaUsername, settings.DeathByCaptchaPassword),
            CaptchaServiceType.CaptchaCoder => new CaptchaCoderService(settings.CaptchaCoderApiKey),
            CaptchaServiceType.ImageTyperz => new ImageTyperzService(settings.ImageTyperzApiKey),
            CaptchaServiceType.CapMonster => new CapMonsterService(string.Empty,
                GetUri(settings.CapMonsterHost, settings.CapMonsterPort)),
            CaptchaServiceType.AzCaptcha => new AzCaptchaService(settings.AZCaptchaApiKey),
            CaptchaServiceType.CaptchasIo => new CaptchasIoService(settings.CaptchasDotIoApiKey),
            CaptchaServiceType.RuCaptcha => new RuCaptchaService(settings.RuCaptchaApiKey),
            CaptchaServiceType.SolveCaptcha => new SolveCaptchaService(settings.SolveCaptchaApiKey),
            CaptchaServiceType.TrueCaptcha => new TrueCaptchaService(settings.TrueCaptchaUsername, settings.TrueCaptchaApiKey),
            CaptchaServiceType.NineKw => new NineKwService(settings.NineKWApiKey),
            CaptchaServiceType.CustomAntiCaptcha => new CustomAntiCaptchaService(settings.CustomAntiCaptchaApiKey,
                GetUri(settings.CustomAntiCaptchaDomain, settings.CustomAntiCaptchaPort)),
            CaptchaServiceType.CapSolver => throw new NotSupportedException(
                "CapSolver itself explicitly asked to be removed from the software. Please choose another service."),
            CaptchaServiceType.CapMonsterCloud => new CapMonsterCloudService(settings.CapMonsterCloudApiKey),
            CaptchaServiceType.HumanCoder => new HumanCoderService(settings.HumanCoderApiKey),
            CaptchaServiceType.Nopecha => new NopechaService(settings.NopechaApiKey),
            CaptchaServiceType.NoCaptchaAi => new NoCaptchaAiService(settings.NoCaptchaAiApiKey),
            CaptchaServiceType.MetaBypassTech => new MetaBypassTechService(settings.MetaBypassTechClientId,
                settings.MetaBypassTechClientSecret, settings.MetaBypassTechUsername, settings.MetaBypassTechPassword),
            CaptchaServiceType.CaptchaAi => new CaptchaAiService(settings.CaptchaAiApiKey),
            CaptchaServiceType.NextCaptcha => new NextCaptchaService(settings.NextCaptchaApiKey),
            CaptchaServiceType.EzCaptcha => new EzCaptchaService(settings.EzCaptchaApiKey),
            CaptchaServiceType.EndCaptcha => new EndCaptchaService(settings.EndCaptchaUsername, settings.EndCaptchaPassword),
            CaptchaServiceType.BestCaptchaSolver => new BestCaptchaSolverService(settings.BestCaptchaSolverApiKey),
            CaptchaServiceType.CapGuru => new CapGuruService(settings.CapGuruApiKey),
            CaptchaServiceType.Aycd => new AycdService(settings.AycdApiKey),
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
