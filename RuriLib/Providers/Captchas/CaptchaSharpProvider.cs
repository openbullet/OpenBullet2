using CaptchaSharp.Enums;
using CaptchaSharp.Models;
using RuriLib.Functions.Captchas;
using RuriLib.Helpers;
using RuriLib.Models.Settings;
using RuriLib.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using CaptchaSharp.Models.CaptchaOptions;
using CaptchaSharp.Models.CaptchaResponses;
using CaptchaSharp.Services;

namespace RuriLib.Providers.Captchas;

/// <summary>
/// Adapter that exposes <see cref="CaptchaSharp.Services.CaptchaService"/> through <see cref="ICaptchaProvider"/>.
/// </summary>
public class CaptchaSharpProvider : ICaptchaProvider
{
    private readonly CaptchaService _service;
    private readonly CaptchaSettings _settings;

    /// <summary>
    /// Creates a captcha provider from the persisted RuriLib settings.
    /// </summary>
    public CaptchaSharpProvider(RuriLibSettingsService settings)
    {
        _settings = Cloner.Clone(settings.RuriLibSettings.CaptchaSettings);
        _service = CaptchaServiceFactory.GetService(_settings);
        CheckBalanceBeforeSolving = _settings.CheckBalanceBeforeSolving;
        ServiceType = _settings.CurrentService;
    }

    /// <inheritdoc />
    public bool CheckBalanceBeforeSolving { get; }
    /// <inheritdoc />
    public CaptchaServiceType ServiceType { get; }
    /// <inheritdoc />
    public TimeSpan Timeout { get => _service.Timeout; set => _service.Timeout = value; }
    /// <inheritdoc />
    public CaptchaServiceCapabilities Capabilities => _service.Capabilities;

    /// <inheritdoc />
    public Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default)
        => CaptchaBalanceCache.GetBalanceAsync(_settings, _service.GetBalanceAsync, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveTextCaptchaAsync(
        string text, TextCaptchaOptions? options = null, CancellationToken cancellationToken = default)
        => _service.SolveTextCaptchaAsync(text, options, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveImageCaptchaAsync(
        string base64, ImageCaptchaOptions? options = null, CancellationToken cancellationToken = default)
        => _service.SolveImageCaptchaAsync(base64, options, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveRecaptchaV2Async(
        string siteKey, string siteUrl, string dataS = "", bool enterprise = false, bool invisible = false,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveRecaptchaV2Async(siteKey, siteUrl, dataS, enterprise, invisible, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveRecaptchaV3Async(
        string siteKey, string siteUrl, string action = "verify", float minScore = 0.4f,
        bool enterprise = false, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveRecaptchaV3Async(siteKey, siteUrl, action, minScore, enterprise, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveFuncaptchaAsync(
        string publicKey, string serviceUrl, string siteUrl,
        bool noJs = false, string? data = null, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default)
        => _service.SolveFuncaptchaAsync(publicKey, serviceUrl, siteUrl, noJs, data, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveHCaptchaAsync(
        string siteKey, string siteUrl, bool invisible = false, string? enterprisePayload = null,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveHCaptchaAsync(siteKey, siteUrl, invisible, enterprisePayload, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveKeyCaptchaAsync(
        string userId, string sessionId, string webServerSign1, string webServerSign2, string siteUrl,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveKeyCaptchaAsync(userId, sessionId, webServerSign1, webServerSign2, siteUrl, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<GeeTestResponse> SolveGeeTestAsync(
        string gt, string challenge, string siteUrl, string? apiServer = null,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveGeeTestAsync(gt, challenge, siteUrl, apiServer, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<CapyResponse> SolveCapyAsync(
        string siteKey, string siteUrl, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveCapyAsync(siteKey, siteUrl, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveDataDomeAsync(
        string siteUrl, string captchaUrl, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveDataDomeAsync(siteUrl, captchaUrl, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<CloudflareTurnstileResponse> SolveCloudflareTurnstileAsync(
        string siteKey, string siteUrl, string? action = null, string? data = null,
        string? pageData = null, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveCloudflareTurnstileAsync(siteKey, siteUrl, action, data, pageData, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<LeminCroppedResponse> SolveLeminCroppedAsync(
        string captchaId, string siteUrl, string apiServer = "https://api.leminnow.com/",
        string? divId = null, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveLeminCroppedAsync(captchaId, siteUrl, apiServer, divId, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveAmazonWafAsync(
        string siteKey, string iv, string context, string siteUrl,
        string? challengeScript = null, string? captchaScript = null,
        SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveAmazonWafAsync(siteKey, iv, context, siteUrl, challengeScript, captchaScript, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveCyberSiAraAsync(
        string masterUrlId, string siteUrl, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveCyberSiAraAsync(masterUrlId, siteUrl, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveMtCaptchaAsync(
        string siteKey, string siteUrl, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveMtCaptchaAsync(siteKey, siteUrl, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveCutCaptchaAsync(
        string miseryKey, string apiKey, string siteUrl, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveCutCaptchaAsync(miseryKey, apiKey, siteUrl, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveFriendlyCaptchaAsync(
        string siteKey, string siteUrl, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveFriendlyCaptchaAsync(siteKey, siteUrl, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveAtbCaptchaAsync(
        string appId, string apiServer, string siteUrl, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveAtbCaptchaAsync(appId, apiServer, siteUrl, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<TencentCaptchaResponse> SolveTencentCaptchaAsync(
        string appId, string siteUrl, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveTencentCaptchaAsync(appId, siteUrl, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveAudioCaptchaAsync(
        string base64, AudioCaptchaOptions? options = null, CancellationToken cancellationToken = default)
        => _service.SolveAudioCaptchaAsync(base64, options, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveRecaptchaMobileAsync(
        string appPackageName, string appKey, string appAction, SessionParams? sessionParams = null,
        CancellationToken cancellationToken = default)
        => _service.SolveRecaptchaMobileAsync(appPackageName, appKey, appAction, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<GeeTestV4Response> SolveGeeTestV4Async(
        string captchaId, string siteUrl, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveGeeTestV4Async(captchaId, siteUrl, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task<StringResponse> SolveCloudflareChallengePageAsync(
        string siteUrl, string pageHtml, SessionParams? sessionParams = null, CancellationToken cancellationToken = default)
        => _service.SolveCloudflareChallengePageAsync(siteUrl, pageHtml, sessionParams, cancellationToken);

    /// <inheritdoc />
    public Task ReportSolutionAsync(string captchaId, CaptchaType type, bool correct,
        CancellationToken cancellationToken = default)
        => _service.ReportSolutionAsync(captchaId, type, correct, cancellationToken);
}
