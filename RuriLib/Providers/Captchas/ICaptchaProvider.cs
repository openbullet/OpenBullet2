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
    /// <summary>
    /// Gets or sets the timeout used by the underlying captcha service.
    /// </summary>
    TimeSpan Timeout { get; set; }

    /// <summary>
    /// Gets a value indicating whether balance should be checked before solving.
    /// </summary>
    bool CheckBalanceBeforeSolving { get; }

    /// <summary>
    /// Gets the configured captcha service type.
    /// </summary>
    CaptchaServiceType ServiceType { get; }

    /// <summary>
    /// Gets the capabilities supported by the service.
    /// </summary>
    CaptchaServiceCapabilities Capabilities { get; }

    /// <summary>
    /// Gets the current account balance.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The remaining balance.</returns>
    Task<decimal> GetBalanceAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a text captcha.
    /// </summary>
    /// <param name="text">The captcha text prompt.</param>
    /// <param name="options">Optional solver-specific settings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The solver response.</returns>
    Task<StringResponse> SolveTextCaptchaAsync(
        string text, TextCaptchaOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves an image captcha.
    /// </summary>
    /// <param name="base64">The base64-encoded image.</param>
    /// <param name="options">Optional solver-specific settings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The solver response.</returns>
    Task<StringResponse> SolveImageCaptchaAsync(
        string base64, ImageCaptchaOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a reCAPTCHA v2 challenge.
    /// </summary>
    Task<StringResponse> SolveRecaptchaV2Async(
        string siteKey, string siteUrl, string dataS = "", bool enterprise = false, bool invisible = false,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a reCAPTCHA v3 challenge.
    /// </summary>
    Task<StringResponse> SolveRecaptchaV3Async(
        string siteKey, string siteUrl, string action = "verify", float minScore = 0.4f,
        bool enterprise = false, SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a FunCaptcha challenge.
    /// </summary>
    Task<StringResponse> SolveFuncaptchaAsync(
        string publicKey, string serviceUrl, string siteUrl,
        bool noJs = false, string? data = null, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves an hCaptcha challenge.
    /// </summary>
    Task<StringResponse> SolveHCaptchaAsync(
        string siteKey, string siteUrl, bool invisible = false, string? enterprisePayload = null,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a KeyCaptcha challenge.
    /// </summary>
    Task<StringResponse> SolveKeyCaptchaAsync(
        string userId, string sessionId, string webServerSign1, string webServerSign2, string siteUrl,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a GeeTest challenge.
    /// </summary>
    Task<GeeTestResponse> SolveGeeTestAsync(
        string gt, string challenge, string siteUrl, string? apiServer = null,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a Capy captcha challenge.
    /// </summary>
    Task<CapyResponse> SolveCapyAsync(
        string siteKey, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a DataDome challenge.
    /// </summary>
    Task<StringResponse> SolveDataDomeAsync(
        string siteUrl, string captchaUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a Cloudflare Turnstile challenge.
    /// </summary>
    Task<CloudflareTurnstileResponse> SolveCloudflareTurnstileAsync(
        string siteKey, string siteUrl, string? action = null, string? data = null,
        string? pageData = null, SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a Lemin Cropped captcha challenge.
    /// </summary>
    Task<LeminCroppedResponse> SolveLeminCroppedAsync(
        string captchaId, string siteUrl, string apiServer = "https://api.leminnow.com/",
        string? divId = null, SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves an Amazon WAF captcha challenge.
    /// </summary>
    Task<StringResponse> SolveAmazonWafAsync(
        string siteKey, string iv, string context, string siteUrl,
        string? challengeScript = null, string? captchaScript = null,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a CyberSiARA captcha challenge.
    /// </summary>
    Task<StringResponse> SolveCyberSiAraAsync(
        string masterUrlId, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves an MTCaptcha challenge.
    /// </summary>
    Task<StringResponse> SolveMtCaptchaAsync(
        string siteKey, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a CutCaptcha challenge.
    /// </summary>
    Task<StringResponse> SolveCutCaptchaAsync(
        string miseryKey, string apiKey, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a FriendlyCaptcha challenge.
    /// </summary>
    Task<StringResponse> SolveFriendlyCaptchaAsync(
        string siteKey, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves an ATB captcha challenge.
    /// </summary>
    Task<StringResponse> SolveAtbCaptchaAsync(
        string appId, string apiServer, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a Tencent captcha challenge.
    /// </summary>
    Task<TencentCaptchaResponse> SolveTencentCaptchaAsync(
        string appId, string siteUrl, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves an audio captcha.
    /// </summary>
    Task<StringResponse> SolveAudioCaptchaAsync(
        string base64, AudioCaptchaOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a mobile reCAPTCHA challenge.
    /// </summary>
    Task<StringResponse> SolveRecaptchaMobileAsync(
        string appPackageName, string appKey, string appAction, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a GeeTest v4 challenge.
    /// </summary>
    Task<GeeTestV4Response> SolveGeeTestV4Async(
        string captchaId, string siteUrl,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solves a Cloudflare challenge page.
    /// </summary>
    Task<StringResponse> SolveCloudflareChallengePageAsync(
        string siteUrl, string pageHtml,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports whether a previously solved captcha was correct.
    /// </summary>
    Task ReportSolutionAsync(
        string id, CaptchaType type, bool correct = false, CancellationToken cancellationToken = default);
}
