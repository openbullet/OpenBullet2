using CaptchaSharp.Enums;
using CaptchaSharp.Models;
using RuriLib.Functions.Captchas;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Providers.Captchas
{
    /// <summary>
    /// Wrapper for <see cref="CaptchaSharp.CaptchaService"/> to avoid exposing API keys.
    /// </summary>
    public interface ICaptchaProvider
    {
        TimeSpan Timeout { get; set; }
        bool CheckBalanceBeforeSolving { get; }
        CaptchaServiceType ServiceType { get; }
        CaptchaServiceCapabilities Capabilities { get; }

        Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default);
        Task<StringResponse> SolveTextCaptchaAsync(string text, TextCaptchaOptions options = null, CancellationToken cancellationToken = default);
        Task<StringResponse> SolveImageCaptchaAsync(string base64, ImageCaptchaOptions options = null, CancellationToken cancellationToken = default);
        Task<StringResponse> SolveRecaptchaV2Async(string siteKey, string siteUrl, string sData = "", bool enterprise = false, bool invisible = false, Proxy proxy = null, CancellationToken cancellationToken = default);
        Task<StringResponse> SolveRecaptchaV3Async(string siteKey, string siteUrl, string action, float minScore, bool enterprise = false, Proxy proxy = null, CancellationToken cancellationToken = default);
        Task<StringResponse> SolveFuncaptchaAsync(string publicKey, string serviceUrl, string siteUrl, bool noJS = false, Proxy proxy = null, CancellationToken cancellationToken = default);
        Task<StringResponse> SolveHCaptchaAsync(string siteKey, string siteUrl, Proxy proxy = null, CancellationToken cancellationToken = default);
        Task<StringResponse> SolveKeyCaptchaAsync(string userId, string sessionId, string webServerSign1, string webServerSign2, string siteUrl, Proxy proxy = null, CancellationToken cancellationToken = default);
        Task<GeeTestResponse> SolveGeeTestAsync(string gt, string challenge, string apiServer, string siteUrl, Proxy proxy = null, CancellationToken cancellationToken = default);
        Task<CapyResponse> SolveCapyAsync(string siteKey, string siteUrl, Proxy proxy = null, CancellationToken cancellationToken = default);
        Task ReportSolution(long id, CaptchaType type, bool correct = false, CancellationToken cancellationToken = default);
    }
}
