using CaptchaSharp.Enums;
using CaptchaSharp.Models;
using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Captchas;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptchaSharp.Models.CaptchaOptions;

namespace RuriLib.Blocks.Captchas;

[BlockCategory("Captchas", "Blocks for solving captchas", "#7df9ff")]
public static class Methods
{
    [Block("Solves a text captcha")]
    public static async Task<string> SolveTextCaptcha(BotData data,
        [BlockParam("Question", "The description of the captcha to solve, e.g. What is 2+2?")] string question,
        CaptchaLanguageGroup languageGroup = CaptchaLanguageGroup.NotSpecified,
        CaptchaLanguage language = CaptchaLanguage.NotSpecified)
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);

        var response = await data.Providers.Captcha.SolveTextCaptchaAsync(question, 
            new TextCaptchaOptions
            { 
                CaptchaLanguage = language,
                CaptchaLanguageGroup = languageGroup 
            }, data.CancellationToken).ConfigureAwait(false);

        AddCaptchaId(data, response.Id, CaptchaType.TextCaptcha);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves an image captcha")]
    public static async Task<string> SolveImageCaptcha(BotData data, string base64,
        CaptchaLanguageGroup languageGroup = CaptchaLanguageGroup.NotSpecified,
        CaptchaLanguage language = CaptchaLanguage.NotSpecified, bool isPhrase = false, bool caseSensitive = true,
        bool requiresCalculation = false, CharacterSet characterSet = CharacterSet.NotSpecified,
        int minLength = 0, int maxLength = 0, string textInstructions = "")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);

        var response = await data.Providers.Captcha.SolveImageCaptchaAsync(base64,
            new ImageCaptchaOptions
            {
                CaptchaLanguage = language,
                CaptchaLanguageGroup = languageGroup,
                IsPhrase = isPhrase,
                CaseSensitive = caseSensitive,
                RequiresCalculation = requiresCalculation,
                CharacterSet = characterSet,
                MinLength = minLength,
                MaxLength = maxLength,
                TextInstructions = textInstructions
            }, data.CancellationToken).ConfigureAwait(false);

        AddCaptchaId(data, response.Id, CaptchaType.ImageCaptcha);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves a ReCaptcha v2")]
    public static async Task<string> SolveRecaptchaV2(BotData data, string siteKey, string siteUrl,
        string sData = "", bool enterprise = false, bool isInvisible = false, bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);

        var response = await data.Providers.Captcha.SolveRecaptchaV2Async(siteKey, siteUrl, sData, enterprise, isInvisible,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

        AddCaptchaId(data, response.Id, CaptchaType.ReCaptchaV2);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    // For backwards compatibility
    public static Task<string> SolveRecaptchaV2(BotData data, string siteKey, string siteUrl, bool isInvisible = false, bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
        => SolveRecaptchaV2(data, siteKey, siteUrl, "", false, isInvisible, useProxy, userAgent);

    [Block("Solves a ReCaptcha v3")]
    public static async Task<string> SolveRecaptchaV3(BotData data, string siteKey, string siteUrl, string action,
        float minScore = 0.3F, bool enterprise = false, bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);

        var response = await data.Providers.Captcha.SolveRecaptchaV3Async(siteKey, siteUrl, action, minScore, enterprise,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

        AddCaptchaId(data, response.Id, CaptchaType.ReCaptchaV3);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    // For backwards compatibility
    public static Task<string> SolveRecaptchaV3(BotData data, string siteKey, string siteUrl, string action, float minScore = 0.3F, bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
        => SolveRecaptchaV3(data, siteKey, siteUrl, action, minScore, false, useProxy, userAgent);

    [Block("Solves a FunCaptcha")]
    public static async Task<string> SolveFunCaptcha(BotData data, string publicKey, string serviceUrl, string siteUrl,
        bool noJS = false, string? extraData = null, bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);

        var response = await data.Providers.Captcha.SolveFuncaptchaAsync(publicKey, serviceUrl, siteUrl, noJS,
            extraData, CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

        AddCaptchaId(data, response.Id, CaptchaType.FunCaptcha);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }
    
    // For backwards compatibility
    public static async Task<string> SolveFunCaptcha(BotData data, string publicKey, string serviceUrl, string siteUrl,
        bool noJS = false, bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
        => await SolveFunCaptcha(data, publicKey, serviceUrl, siteUrl, noJS, extraData: null, useProxy, userAgent);

    [Block("Solves a HCaptcha")]
    public static async Task<string> SolveHCaptcha(BotData data, string siteKey, string siteUrl, 
        string? enterprisePayload = null, bool isInvisible = false, bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);

        var response = await data.Providers.Captcha.SolveHCaptchaAsync(siteKey, siteUrl, isInvisible, enterprisePayload,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

        AddCaptchaId(data, response.Id, CaptchaType.HCaptcha);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }
    
    // For backwards compatibility
    public static async Task<string> SolveHCaptcha(BotData data, string siteKey, string siteUrl,
        bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
        => await SolveHCaptcha(data, siteKey, siteUrl, enterprisePayload: null, isInvisible: false, useProxy, userAgent);

    [Block("Solves a KeyCaptcha")]
    public static async Task<string> SolveKeyCaptcha(BotData data, string userId, string sessionId,
        string webServerSign1, string webServerSign2, string siteUrl, bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);

        var response = await data.Providers.Captcha.SolveKeyCaptchaAsync(userId, sessionId, webServerSign1, webServerSign2, siteUrl,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

        AddCaptchaId(data, response.Id, CaptchaType.KeyCaptcha);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves a GeeTest v3 captcha",
        extraInfo = "The response will be a list and its elements are (in order) challenge, validate, seccode")]
    public static async Task<List<string>> SolveGeeTestCaptcha(BotData data, string gt, string apiChallenge,
        string apiServer, string siteUrl, bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);

        var response = await data.Providers.Captcha.SolveGeeTestAsync(gt, apiChallenge, siteUrl, apiServer,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

        AddCaptchaId(data, response.Id, CaptchaType.GeeTest);
        data.Logger.Log("Got solution!", LogColors.ElectricBlue);
        data.Logger.Log($"Challenge: {response.Challenge}", LogColors.ElectricBlue);
        data.Logger.Log($"Validate: {response.Validate}", LogColors.ElectricBlue);
        data.Logger.Log($"SecCode: {response.SecCode}", LogColors.ElectricBlue);
        return [response.Challenge, response.Validate, response.SecCode];
    }
    
    [Block("Solves a Capy captcha")]
    public static async Task<List<string>> SolveCapyCaptcha(BotData data, string siteKey, string siteUrl,
        bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);

        var response = await data.Providers.Captcha.SolveCapyAsync(siteKey, siteUrl,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

        AddCaptchaId(data, response.Id, CaptchaType.Capy);
        data.Logger.Log("Got solution!", LogColors.ElectricBlue);
        data.Logger.Log($"Challenge Key: {response.ChallengeKey}", LogColors.ElectricBlue);
        data.Logger.Log($"Captcha Key: {response.CaptchaKey}", LogColors.ElectricBlue);
        data.Logger.Log($"Answer: {response.Answer}", LogColors.ElectricBlue);
        return [response.ChallengeKey, response.CaptchaKey, response.Answer];
    }
    
    [Block("Solves a DataDome captcha")]
    public static async Task<string> SolveDataDomeCaptcha(BotData data, string captchaUrl, string siteUrl,
        bool useProxy = false,
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);

        var response = await data.Providers.Captcha.SolveDataDomeAsync(siteUrl, captchaUrl,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

        AddCaptchaId(data, response.Id, CaptchaType.DataDome);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves a Cloudflare Turnstile captcha")]
    public static async Task<string> SolveCloudflareTurnstileCaptcha(BotData data, string siteKey, string siteUrl,
        string? action = null, string? cData = null, string? pageData = null, bool useProxy = false,
        string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveCloudflareTurnstileAsync(siteKey, siteUrl, action, cData, pageData,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.CloudflareTurnstile);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves a Lemin Cropped captcha",
        extraInfo = "The response will be a list and its elements are (in order) answer, challenge ID")]
    public static async Task<List<string>> SolveLeminCroppedCaptcha(BotData data, string captchaId, string siteUrl, 
        string apiServer = "https://api.leminnow.com/", string? divId = null, bool useProxy = false,
        string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveLeminCroppedAsync(captchaId, siteUrl, apiServer, divId,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.LeminCropped);
        data.Logger.Log("Got solution!", LogColors.ElectricBlue);
        data.Logger.Log($"Answer: {response.Answer}", LogColors.ElectricBlue);
        data.Logger.Log($"Challenge ID: {response.ChallengeId}", LogColors.ElectricBlue);
        return [response.Answer, response.ChallengeId];
    }
    
    [Block("Solves an Amazon WAF captcha")]
    public static async Task<string> SolveAmazonWafCaptcha(BotData data, string siteKey, string siteUrl,
        string iv, string context, string? challengeScript = null, string? captchaScript = null, bool useProxy = false,
        string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveAmazonWafAsync(siteKey, iv, context, siteUrl, challengeScript, 
            captchaScript, CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.AmazonWaf);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves a Cyber SiARA captcha")]
    public static async Task<string> SolveCyberSiAraCaptcha(BotData data, string masterUrlId, string siteUrl,
        bool useProxy = false,
        string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveCyberSiAraAsync(masterUrlId, siteUrl,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.CyberSiAra);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves an MT captcha")]
    public static async Task<string> SolveMtCaptcha(BotData data, string siteKey, string siteUrl,
        bool useProxy = false, string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveMtCaptchaAsync(siteKey, siteUrl,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.MtCaptcha);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves a CutCaptcha")]
    public static async Task<string> SolveCutCaptcha(BotData data, string miseryKey, string apiKey, string siteUrl,
        bool useProxy = false, string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveCutCaptchaAsync(miseryKey, apiKey, siteUrl,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.CutCaptcha);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves a Friendly captcha")]
    public static async Task<string> SolveFriendlyCaptcha(BotData data, string siteKey, string siteUrl,
        bool useProxy = false, string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveFriendlyCaptchaAsync(siteKey, siteUrl,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.FriendlyCaptcha);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves an atb captcha")]
    public static async Task<string> SolveAtbCaptcha(BotData data, string appId, string apiServer, string siteUrl,
        bool useProxy = false, string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveAtbCaptchaAsync(appId, apiServer, siteUrl,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.AtbCaptcha);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves a Tencent captcha",
        extraInfo = "The response will be a list and its elements are (in order) app id, ticket, return code, random string")]
    public static async Task<List<string>> SolveTencentCaptcha(BotData data, string appId, string siteUrl,
        bool useProxy = false, string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveTencentCaptchaAsync(appId, siteUrl,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.TencentCaptcha);
        data.Logger.Log("Got solution!", LogColors.ElectricBlue);
        data.Logger.Log($"App ID: {response.AppId}", LogColors.ElectricBlue);
        data.Logger.Log($"Ticket: {response.Ticket}", LogColors.ElectricBlue);
        data.Logger.Log($"Return Code: {response.ReturnCode}", LogColors.ElectricBlue);
        data.Logger.Log($"Random String: {response.RandomString}", LogColors.ElectricBlue);
        return [response.AppId, response.Ticket, response.ReturnCode.ToString(), response.RandomString];
    }

    [Block("Solves an audio captcha")]
    public static async Task<string> SolveAudioCaptcha(BotData data, string base64,
        CaptchaLanguage language = CaptchaLanguage.NotSpecified)
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveAudioCaptchaAsync(base64, new AudioCaptchaOptions
        {
            CaptchaLanguage = language
        }, data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.AudioCaptcha);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves a ReCaptcha Mobile")]
    public static async Task<string> SolveRecaptchaMobile(BotData data, string appPackageName, string appKey,
        string appAction, bool useProxy = false, string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveRecaptchaMobileAsync(appPackageName, appKey, appAction,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.ReCaptchaMobile);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Solves a GeeTest v4 captcha",
        extraInfo =
            "The response will be a list and its elements are (in order) captcha id, lot number, pass token, gen time, captcha output")]
    public static async Task<List<string>> SolveGeeTestV4Captcha(BotData data, string captchaId,
        string siteUrl, bool useProxy = false, string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveGeeTestV4Async(captchaId, siteUrl,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.GeeTestV4);
        data.Logger.Log("Got solution!", LogColors.ElectricBlue);
        data.Logger.Log($"Captcha ID: {response.CaptchaId}", LogColors.ElectricBlue);
        data.Logger.Log($"Lot Number: {response.LotNumber}", LogColors.ElectricBlue);
        data.Logger.Log($"Pass Token: {response.PassToken}", LogColors.ElectricBlue);
        data.Logger.Log($"Gen Time: {response.GenTime}", LogColors.ElectricBlue);
        data.Logger.Log($"Captcha Output: {response.CaptchaOutput}", LogColors.ElectricBlue);
        return [response.CaptchaId, response.LotNumber, response.PassToken, response.GenTime, response.CaptchaOutput];
    }

    [Block("Solves a Cloudflare Challenge page",
        extraInfo = "The response will contain the value of the cf_clearance cookie")]
    public static async Task<string> SolveCloudflareChallengePage(BotData data,
        string siteUrl, string pageHtml, bool useProxy = false, string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36")
    {
        data.Logger.LogHeader();
        await CheckBalanceAsync(data).ConfigureAwait(false);
        
        var response = await data.Providers.Captcha.SolveCloudflareChallengePageAsync(
            siteUrl, pageHtml,
            CreateSessionParams(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);
        
        AddCaptchaId(data, response.Id, CaptchaType.CloudflareChallengePage);
        data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
        return response.Response;
    }

    [Block("Reports an incorrectly solved captcha to the service in order to get funds back")]
    public static async Task ReportLastSolution(BotData data)
    {
        var lastCaptcha = data.TryGetObject<CaptchaInfo>("lastCaptchaInfo");

        data.Logger.LogHeader();
        
        if (lastCaptcha is null)
        {
            data.Logger.Log("No captcha has been solved yet", LogColors.ElectricBlue);
            return;
        }

        try
        {
            await data.Providers.Captcha.ReportSolutionAsync(
                lastCaptcha.Id, lastCaptcha.Type, false, data.CancellationToken).ConfigureAwait(false);
            
            data.Logger.Log($"Solution of task {lastCaptcha.Id} reported correctly!", LogColors.ElectricBlue);
        }
        catch (Exception ex)
        {
            data.Logger.Log(
                $"Could not report the solution of task {lastCaptcha.Id} to the service: {ex.Message}",
                LogColors.ElectricBlue);
        }
    }

    private static async Task CheckBalanceAsync(BotData data)
    {
        if (!data.Providers.Captcha.CheckBalanceBeforeSolving)
        {
            return;
        }

        try
        {
            data.CaptchaCredit = await data.Providers.Captcha.GetBalanceAsync(data.CancellationToken).ConfigureAwait(false);
            data.Logger.Log($"[{data.Providers.Captcha.ServiceType}] Balance: ${data.CaptchaCredit}", LogColors.ElectricBlue);

            if (data.CaptchaCredit < (decimal)0.002)
                throw new Exception("The remaining balance is too low!");
        }
        catch (Exception ex) // This unwraps aggregate exceptions
        {
            if (ex is AggregateException { InnerException: not null } aggEx)
            {
                throw aggEx.InnerException;
            }
            
            throw;
        }
    }

    private static void AddCaptchaId(BotData data, string id, CaptchaType type)
        => data.SetObject("lastCaptchaInfo", new CaptchaInfo { Id = id, Type = type });

    private static SessionParams CreateSessionParams(BotData data, bool useProxy, string userAgent)
    {
        var proxy = data.UseProxy && useProxy && data.Proxy is not null
            ? new Proxy
            {
                Host = data.Proxy.Host,
                Port = data.Proxy.Port,
                Type = Enum.Parse<ProxyType>(data.Proxy.Type.ToString(), true),
                Username = data.Proxy.Username,
                Password = data.Proxy.Password
            }
            : null;

        return new SessionParams
        {
            Proxy = proxy,
            UserAgent = userAgent,
            Cookies = data.COOKIES
        };
    }
}
