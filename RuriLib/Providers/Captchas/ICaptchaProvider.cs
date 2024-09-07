using CaptchaSharp.Enums;
using CaptchaSharp.Models;
using RuriLib.Functions.Captchas;
using System;
using System.Threading;
using System.Threading.Tasks;
using CaptchaSharp.Models.CaptchaOptions;
using CaptchaSharp.Models.CaptchaResponses;
using CaptchaSharp.Services;

namespace RuriLib.Providers.Captchas;

/// <summary>
/// Wrapper for <see cref="CaptchaService"/> to avoid exposing API keys.
/// </summary>
public interface ICaptchaProvider
{
    TimeSpan Timeout { get; set; }
    bool CheckBalanceBeforeSolving { get; }
    CaptchaServiceType ServiceType { get; }
    CaptchaServiceCapabilities Capabilities { get; }

    Task<decimal> GetBalanceAsync(
        CancellationToken cancellationToken = default);

    Task<StringResponse> SolveTextCaptchaAsync(
        string text, TextCaptchaOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<StringResponse> SolveImageCaptchaAsync(
        string base64, ImageCaptchaOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<StringResponse> SolveRecaptchaV2Async(
        string siteKey, string siteUrl, string dataS = "", bool enterprise = false, bool invisible = false,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    Task<StringResponse> SolveRecaptchaV3Async(
        string siteKey, string siteUrl, string action = "verify", float minScore = 0.4f,
        bool enterprise = false, SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    Task<StringResponse> SolveFuncaptchaAsync(
        string publicKey, string serviceUrl, string siteUrl,
        bool noJs = false, string? data = null, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    Task<StringResponse> SolveHCaptchaAsync(
        string siteKey, string siteUrl, bool invisible = false, string? enterprisePayload = null,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    Task<StringResponse> SolveKeyCaptchaAsync(
        string userId, string sessionId, string webServerSign1, string webServerSign2, string siteUrl,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    Task<GeeTestResponse> SolveGeeTestAsync(
        string gt, string challenge, string siteUrl, string? apiServer = null,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    Task<CapyResponse> SolveCapyAsync(
        string siteKey, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    Task<StringResponse> SolveDataDomeAsync(
        string siteUrl, string captchaUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    Task<CloudflareTurnstileResponse> SolveCloudflareTurnstileAsync(
        string siteKey, string siteUrl, string? action = null, string? data = null,
        string? pageData = null, SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    Task<LeminCroppedResponse> SolveLeminCroppedAsync(
        string captchaId, string siteUrl, string apiServer = "https://api.leminnow.com/",
        string? divId = null, SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    Task<StringResponse> SolveAmazonWafAsync(
        string siteKey, string iv, string context, string siteUrl,
        string? challengeScript = null, string? captchaScript = null,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    Task<StringResponse> SolveCyberSiAraAsync(
        string masterUrlId, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    Task<StringResponse> SolveMtCaptchaAsync(
        string siteKey, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    Task<StringResponse> SolveCutCaptchaAsync(
        string miseryKey, string apiKey, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    Task<StringResponse> SolveFriendlyCaptchaAsync(
        string siteKey, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    Task<StringResponse> SolveAtbCaptchaAsync(
        string appId, string apiServer, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    Task<TencentCaptchaResponse> SolveTencentCaptchaAsync(
        string appId, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    Task<StringResponse> SolveAudioCaptchaAsync(
        string base64, AudioCaptchaOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<StringResponse> SolveRecaptchaMobileAsync(
        string appPackageName, string appKey, string appAction, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    Task<GeeTestV4Response> SolveGeeTestV4Async(
        string captchaId, string siteUrl,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    Task<StringResponse> SolveCloudflareChallengePageAsync(
        string siteUrl, string pageHtml,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    Task ReportSolutionAsync(
        string id, CaptchaType type, bool correct = false, CancellationToken cancellationToken = default);
}
