using CaptchaSharp.Enums;
using CaptchaSharp.Models;
using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Captchas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Captchas
{
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
            await CheckBalance(data).ConfigureAwait(false);

            var response = await data.Providers.Captcha.SolveTextCaptchaAsync(question, 
                new TextCaptchaOptions
                { 
                    CaptchaLanguage = language,
                    CaptchaLanguageGroup = languageGroup 
                }, data.CancellationToken).ConfigureAwait(false);

            AddCaptchaId(data, response.IdString, CaptchaType.TextCaptcha);
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
            await CheckBalance(data).ConfigureAwait(false);

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

            AddCaptchaId(data, response.IdString, CaptchaType.ImageCaptcha);
            data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
            return response.Response;
        }

        [Block("Solves a ReCaptcha V2")]
        public static async Task<string> SolveRecaptchaV2(BotData data, string siteKey, string siteUrl,
            string sData = "", bool enterprise = false, bool isInvisible = false, bool useProxy = false,
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36")
        {
            data.Logger.LogHeader();
            await CheckBalance(data).ConfigureAwait(false);

            var response = await data.Providers.Captcha.SolveRecaptchaV2Async(siteKey, siteUrl, sData, enterprise, isInvisible,
                SetupProxy(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

            AddCaptchaId(data, response.IdString, CaptchaType.ReCaptchaV2);
            data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
            return response.Response;
        }

        // For backwards compatibility
        public static Task<string> SolveRecaptchaV2(BotData data, string siteKey, string siteUrl, bool isInvisible = false, bool useProxy = false,
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36")
            => SolveRecaptchaV2(data, siteKey, siteUrl, "", false, isInvisible, useProxy, userAgent);

        [Block("Solves a ReCaptcha V3")]
        public static async Task<string> SolveRecaptchaV3(BotData data, string siteKey, string siteUrl, string action,
            float minScore = 0.3F, bool enterprise = false, bool useProxy = false,
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36")
        {
            data.Logger.LogHeader();
            await CheckBalance(data).ConfigureAwait(false);

            var response = await data.Providers.Captcha.SolveRecaptchaV3Async(siteKey, siteUrl, action, minScore, enterprise,
                SetupProxy(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

            AddCaptchaId(data, response.IdString, CaptchaType.ReCaptchaV3);
            data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
            return response.Response;
        }

        // For backwards compatibility
        public static Task<string> SolveRecaptchaV3(BotData data, string siteKey, string siteUrl, string action, float minScore = 0.3F, bool useProxy = false,
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36")
            => SolveRecaptchaV3(data, siteKey, siteUrl, action, minScore, false, useProxy, userAgent);

        [Block("Solves a FunCaptcha")]
        public static async Task<string> SolveFunCaptcha(BotData data, string publicKey, string serviceUrl, string siteUrl,
            bool noJS = false, bool useProxy = false,
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36")
        {
            data.Logger.LogHeader();
            await CheckBalance(data).ConfigureAwait(false);

            var response = await data.Providers.Captcha.SolveFuncaptchaAsync(publicKey, serviceUrl, siteUrl, noJS,
                SetupProxy(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

            AddCaptchaId(data, response.IdString, CaptchaType.FunCaptcha);
            data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
            return response.Response;
        }

        [Block("Solves a HCaptcha")]
        public static async Task<string> SolveHCaptcha(BotData data, string siteKey, string siteUrl,
            bool useProxy = false,
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36")
        {
            data.Logger.LogHeader();
            await CheckBalance(data).ConfigureAwait(false);

            var response = await data.Providers.Captcha.SolveHCaptchaAsync(siteKey, siteUrl,
                SetupProxy(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

            AddCaptchaId(data, response.IdString, CaptchaType.HCaptcha);
            data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
            return response.Response;
        }

        [Block("Solves a Capy captcha")]
        public static async Task<List<string>> SolveCapyCaptcha(BotData data, string siteKey, string siteUrl,
            bool useProxy = false,
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36")
        {
            data.Logger.LogHeader();
            await CheckBalance(data).ConfigureAwait(false);

            var response = await data.Providers.Captcha.SolveCapyAsync(siteKey, siteUrl,
                SetupProxy(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

            AddCaptchaId(data, response.IdString, CaptchaType.Capy);
            data.Logger.Log($"Got solution!", LogColors.ElectricBlue);
            data.Logger.Log($"Challenge Key: {response.ChallengeKey}", LogColors.ElectricBlue);
            data.Logger.Log($"Captcha Key: {response.CaptchaKey}", LogColors.ElectricBlue);
            data.Logger.Log($"Answer: {response.Answer}", LogColors.ElectricBlue);
            return new List<string> { response.ChallengeKey, response.CaptchaKey, response.Answer };
        }

        [Block("Solves a KeyCaptcha")]
        public static async Task<string> SolveKeyCaptcha(BotData data, string userId, string sessionId,
            string webServerSign1, string webServerSign2, string siteUrl, bool useProxy = false,
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36")
        {
            data.Logger.LogHeader();
            await CheckBalance(data).ConfigureAwait(false);

            var response = await data.Providers.Captcha.SolveKeyCaptchaAsync(userId, sessionId, webServerSign1, webServerSign2, siteUrl,
                SetupProxy(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

            AddCaptchaId(data, response.IdString, CaptchaType.KeyCaptcha);
            data.Logger.Log($"Got solution: {response.Response}", LogColors.ElectricBlue);
            return response.Response;
        }

        [Block("Solves a GeeTest captcha",
            extraInfo = "The response will be a list and its elements are (in order) challenge, validate, seccode")]
        public static async Task<List<string>> SolveGeeTestCaptcha(BotData data, string gt, string apiChallenge,
            string apiServer, string siteUrl, bool useProxy = false,
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36")
        {
            data.Logger.LogHeader();
            await CheckBalance(data).ConfigureAwait(false);

            var response = await data.Providers.Captcha.SolveGeeTestAsync(gt, apiChallenge, apiServer, siteUrl,
                SetupProxy(data, useProxy, userAgent), data.CancellationToken).ConfigureAwait(false);

            AddCaptchaId(data, response.IdString, CaptchaType.GeeTest);
            data.Logger.Log($"Got solution!", LogColors.ElectricBlue);
            data.Logger.Log($"Challenge: {response.Challenge}", LogColors.ElectricBlue);
            data.Logger.Log($"Validate: {response.Validate}", LogColors.ElectricBlue);
            data.Logger.Log($"SecCode: {response.SecCode}", LogColors.ElectricBlue);
            return new List<string> { response.Challenge, response.Validate, response.SecCode };
        }

        [Block("Reports an incorrectly solved captcha to the service in order to get funds back")]
        public static async Task ReportLastSolution(BotData data)
        {
            var lastCaptcha = data.TryGetObject<CaptchaInfo>("lastCaptchaInfo");

            data.Logger.LogHeader();

            try
            {
                // TODO: Create a ReportSolution method that accepts strings in CaptchaSharp! For now we will do it like this
                // since the only service which has string-based captcha IDs does not support reporting bad solutions.
                await data.Providers.Captcha.ReportSolution(long.Parse(lastCaptcha.Id), lastCaptcha.Type, false, data.CancellationToken).ConfigureAwait(false);
                data.Logger.Log($"Solution of task {lastCaptcha.Id} reported correctly!", LogColors.ElectricBlue);
            }
            catch (Exception ex)
            {
                data.Logger.Log($"Could not report the solution of task {lastCaptcha.Id} to the service: {ex.Message}", LogColors.ElectricBlue);
            }
        }

        private static async Task CheckBalance(BotData data)
        {
            if (!data.Providers.Captcha.CheckBalanceBeforeSolving)
                return;

            try
            {
                data.CaptchaCredit = await data.Providers.Captcha.GetBalanceAsync(data.CancellationToken).ConfigureAwait(false);
                data.Logger.Log($"[{data.Providers.Captcha.ServiceType}] Balance: ${data.CaptchaCredit}", LogColors.ElectricBlue);

                if (data.CaptchaCredit < (decimal)0.002)
                    throw new Exception("The remaining balance is too low!");
            }
            catch (Exception ex) // This unwraps aggregate exceptions
            {
                if (ex is AggregateException) throw ex.InnerException;
                else throw;
            }
        }

        private static void AddCaptchaId(BotData data, string id, CaptchaType type)
            => data.SetObject("lastCaptchaInfo", new CaptchaInfo { Id = id, Type = type });

        private static Proxy SetupProxy(BotData data, bool useProxy, string userAgent) 
            => data.UseProxy && useProxy
                ? new Proxy
                {
                    Host = data.Proxy.Host,
                    Port = data.Proxy.Port,
                    Type = Enum.Parse<ProxyType>(data.Proxy.Type.ToString(), true),
                    Username = data.Proxy.Username,
                    Password = data.Proxy.Password,
                    UserAgent = userAgent,
                    Cookies = data.COOKIES
                        .Select(c => (c.Key, c.Value)).ToArray()
                }
                : null;
    }
}
