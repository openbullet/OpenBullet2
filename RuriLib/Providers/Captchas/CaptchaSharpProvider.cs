using CaptchaSharp;
using CaptchaSharp.Enums;
using CaptchaSharp.Models;
using RuriLib.Functions.Captchas;
using RuriLib.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Providers.Captchas
{
    public class CaptchaSharpProvider : ICaptchaProvider
    {
        private readonly CaptchaService service;

        public CaptchaSharpProvider(RuriLibSettingsService settings)
        {
            service = CaptchaServiceFactory.GetService(settings.RuriLibSettings.CaptchaSettings);
            CheckBalanceBeforeSolving = settings.RuriLibSettings.CaptchaSettings.CheckBalanceBeforeSolving;
            ServiceType = settings.RuriLibSettings.CaptchaSettings.CurrentService;
        }

        public bool CheckBalanceBeforeSolving { get; }
        public CaptchaServiceType ServiceType { get; }
        public TimeSpan Timeout { get => service.Timeout; set => service.Timeout = value; }
        public CaptchaServiceCapabilities Capabilities => service.Capabilities;

        public Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default)
            => service.GetBalanceAsync(cancellationToken);

        public Task ReportSolution(long id, CaptchaType type, bool correct = false, CancellationToken cancellationToken = default)
            => service.ReportSolution(id, type, correct, cancellationToken);

        public Task<CapyResponse> SolveCapyAsync(string siteKey, string siteUrl, Proxy proxy = null, CancellationToken cancellationToken = default)
            => service.SolveCapyAsync(siteKey, siteUrl, proxy, cancellationToken);

        public Task<StringResponse> SolveFuncaptchaAsync(string publicKey, string serviceUrl, string siteUrl,
            bool noJS = false, Proxy proxy = null, CancellationToken cancellationToken = default)
            => service.SolveFuncaptchaAsync(publicKey, serviceUrl, siteUrl, noJS, proxy, cancellationToken);

        public Task<GeeTestResponse> SolveGeeTestAsync(string gt, string challenge, string apiServer, string siteUrl,
            Proxy proxy = null, CancellationToken cancellationToken = default)
            => service.SolveGeeTestAsync(gt, challenge, apiServer, siteUrl, proxy, cancellationToken);

        public Task<StringResponse> SolveHCaptchaAsync(string siteKey, string siteUrl, Proxy proxy = null, CancellationToken cancellationToken = default)
            => service.SolveHCaptchaAsync(siteKey, siteUrl, proxy, cancellationToken);

        public Task<StringResponse> SolveImageCaptchaAsync(string base64, ImageCaptchaOptions options = null, CancellationToken cancellationToken = default)
            => service.SolveImageCaptchaAsync(base64, options, cancellationToken);

        public Task<StringResponse> SolveKeyCaptchaAsync(string userId, string sessionId, string webServerSign1,
            string webServerSign2, string siteUrl, Proxy proxy = null, CancellationToken cancellationToken = default)
            => service.SolveKeyCaptchaAsync(userId, sessionId, webServerSign1, webServerSign2, siteUrl, proxy, cancellationToken);

        public Task<StringResponse> SolveRecaptchaV2Async(string siteKey, string siteUrl, string sData = "",
            bool enterprise = false, bool invisible = false, Proxy proxy = null, CancellationToken cancellationToken = default)
            => service.SolveRecaptchaV2Async(siteKey, siteUrl, sData, enterprise, invisible, proxy, cancellationToken);

        public Task<StringResponse> SolveRecaptchaV3Async(string siteKey, string siteUrl, string action, float minScore,
            bool enterprise = false, Proxy proxy = null, CancellationToken cancellationToken = default)
            => service.SolveRecaptchaV3Async(siteKey, siteUrl, action, minScore, proxy, cancellationToken);

        public Task<StringResponse> SolveTextCaptchaAsync(string text, TextCaptchaOptions options = null, CancellationToken cancellationToken = default)
            => service.SolveTextCaptchaAsync(text, options, cancellationToken);
    }
}
