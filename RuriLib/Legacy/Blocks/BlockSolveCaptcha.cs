using RuriLib.LS;
using RuriLib.Utils.Parsing;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using CaptchaSharp.Enums;
using RuriLib.Functions.Captchas;
using CaptchaSharp.Exceptions;
using System.Windows.Media;
using CaptchaSharp.Models;
using CaptchaSharp;

namespace RuriLib
{
    /// <summary>
    /// A block that solves a captcha challenge using a remote solving service.
    /// </summary>
    public class BlockSolveCaptcha : BlockBase
    {
        #region General
        private CaptchaType type = CaptchaType.ReCaptchaV2;
        /// <summary>The type of captcha to solve.</summary>
        public CaptchaType Type { get { return type; } set { type = value; OnPropertyChanged(); } }

        private bool useProxy = false;
        /// <summary>Whether to tell the service to use your proxy to solve captchas.</summary>
        public bool UseProxy { get { return useProxy; } set { useProxy = value; OnPropertyChanged(); } }

        private string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36";
        /// <summary>The user agent that the service will use (should be the same as yours).</summary>
        public string UserAgent { get { return userAgent; } set { userAgent = value; OnPropertyChanged(); } }
        #endregion

        #region TextCaptcha
        private string question = "";
        /// <summary>The captcha question to answer.</summary>
        public string Question { get { return question; } set { question = value; OnPropertyChanged(); } }

        private CaptchaLanguageGroup languageGroup = CaptchaLanguageGroup.NotSpecified;
        /// <summary>The language group of the captcha text.</summary>
        public CaptchaLanguageGroup LanguageGroup { get { return languageGroup; } set { languageGroup = value; OnPropertyChanged(); } }

        private CaptchaLanguage language = CaptchaLanguage.NotSpecified;
        /// <summary>The language group of the captcha text.</summary>
        public CaptchaLanguage Language { get { return language; } set { language = value; OnPropertyChanged(); } }
        #endregion

        #region ImageCaptcha
        private string base64 = "";
        /// <summary>The captcha image as a base64 encoded string.</summary>
        public string Base64 { get { return base64; } set { base64 = value; OnPropertyChanged(); } }

        private bool isPhrase = false;
        /// <summary>Whether the captcha has multiple words.</summary>
        public bool IsPhrase { get { return isPhrase; } set { isPhrase = value; OnPropertyChanged(); } }

        private bool caseSensitive = false;
        /// <summary>Whether the captcha solution should be case sensitive.</summary>
        public bool CaseSensitive { get { return caseSensitive; } set { caseSensitive = value; OnPropertyChanged(); } }

        private CharacterSet charSet = CharacterSet.NotSpecified;
        /// <summary>The type of characters that can appear in the image.</summary>
        public CharacterSet CharSet { get { return charSet; } set { charSet = value; OnPropertyChanged(); } }

        private bool requiresCalculation = false;
        /// <summary>Whether the captcha requires mathematical calculations.</summary>
        public bool RequiresCalculation { get { return requiresCalculation; } set { requiresCalculation = value; OnPropertyChanged(); } }

        private int minLength = 0;
        /// <summary>The minimum length of the solution (0 is unspecified).</summary>
        public int MinLength { get { return minLength; } set { minLength = value; OnPropertyChanged(); } }

        private int maxLength = 0;
        /// <summary>The maximum length of the solution (0 is unspecified).</summary>
        public int MaxLength { get { return maxLength; } set { maxLength = value; OnPropertyChanged(); } }

        private string textInstructions = "";
        /// <summary>Any additional instructions useful to the solution.</summary>
        public string TextInstructions { get { return textInstructions; } set { textInstructions = value; OnPropertyChanged(); } }
        #endregion

        #region Token Captchas
        private string siteKey = "";
        /// <summary>The site key.</summary>
        public string SiteKey { get { return siteKey; } set { siteKey = value; OnPropertyChanged(); } }

        private string siteUrl = "";
        /// <summary>The site URL.</summary>
        public string SiteUrl { get { return siteUrl; } set { siteUrl = value; OnPropertyChanged(); } }

        private bool isInvisible = false;
        /// <summary>Whether the ReCaptchaV2 is invisible.</summary>
        public bool IsInvisible { get { return isInvisible; } set { isInvisible = value; OnPropertyChanged(); } }

        private string action = "";
        /// <summary>The ReCaptchaV3 action.</summary>
        public string Action { get { return action; } set { action = value; OnPropertyChanged(); } }

        private string minScore = "0.3";
        /// <summary>The ReCaptchaV3 minimum required score.</summary>
        public string MinScore { get { return minScore; } set { minScore = value; OnPropertyChanged(); } }
        #endregion

        #region FunCaptcha
        private string publicKey = "";
        /// <summary>The public key of the website.</summary>
        public string PublicKey { get { return publicKey; } set { publicKey = value; OnPropertyChanged(); } }

        private string serviceUrl = "";
        /// <summary>The service URL of the website.</summary>
        public string ServiceUrl { get { return serviceUrl; } set { serviceUrl = value; OnPropertyChanged(); } }

        private bool noJS = false;
        /// <summary>Whether to solve the FunCaptcha with JS disabled.</summary>
        public bool NoJS { get { return noJS; } set { noJS = value; OnPropertyChanged(); } }
        #endregion

        #region KeyCaptcha
        private string userId = "";
        /// <summary>The user ID.</summary>
        public string UserId { get { return userId; } set { userId = value; OnPropertyChanged(); } }

        private string sessionId = "";
        /// <summary>The session ID.</summary>
        public string SessionId { get { return sessionId; } set { sessionId = value; OnPropertyChanged(); } }

        private string webServerSign1 = "";
        /// <summary>The WebServerSign1.</summary>
        public string WebServerSign1 { get { return webServerSign1; } set { webServerSign1 = value; OnPropertyChanged(); } }

        private string webServerSign2 = "";
        /// <summary>The WebServerSign2.</summary>
        public string WebServerSign2 { get { return webServerSign2; } set { webServerSign2 = value; OnPropertyChanged(); } }
        #endregion

        #region GeeTest
        private string gt = "";
        /// <summary>The gt static key.</summary>
        public string GT { get { return gt; } set { gt = value; OnPropertyChanged(); } }

        private string challenge = "";
        /// <summary>The challenge dynamic key.</summary>
        public string Challenge { get { return challenge; } set { challenge = value; OnPropertyChanged(); } }

        private string apiServer = "";
        /// <summary>The api server domain.</summary>
        public string ApiServer { get { return apiServer; } set { apiServer = value; OnPropertyChanged(); } }
        #endregion

        /// <summary>
        /// Creates a SolveCaptcha block.
        /// </summary>
        public BlockSolveCaptcha()
        {
            Label = "SOLVE CAPTCHA";
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

            switch (Type)
            {
                case CaptchaType.TextCaptcha:
                    // SOLVECAPTCHA TextCaptcha "question?" LANGUAGEGROUP LANGUAGE
                    Question = LineParser.ParseLiteral(ref input, "QUESTION");
                    LanguageGroup = (CaptchaLanguageGroup)LineParser.ParseEnum(ref input, "LANG GROUP", typeof(CaptchaLanguageGroup));
                    Language = (CaptchaLanguage)LineParser.ParseEnum(ref input, "LANG", typeof(CaptchaLanguage));
                    break;

                case CaptchaType.ImageCaptcha:
                    // SOLVECAPTCHA ImageCaptcha "base64" LANGUAGEGROUP LANGUAGE MINLEN MAXLEN CHARSET "instructions"
                    // [IsPhrase?] [CaseSensitive?] [RequiresCalculation?]
                    Base64 = LineParser.ParseLiteral(ref input, "BASE64");
                    LanguageGroup = (CaptchaLanguageGroup)LineParser.ParseEnum(ref input, "LANG GROUP", typeof(CaptchaLanguageGroup));
                    Language = (CaptchaLanguage)LineParser.ParseEnum(ref input, "LANG", typeof(CaptchaLanguage));
                    MinLength = LineParser.ParseInt(ref input, "MIN LEN");
                    MaxLength = LineParser.ParseInt(ref input, "MAX LEN");
                    CharSet = (CharacterSet)LineParser.ParseEnum(ref input, "CHARSET", typeof(CharacterSet));
                    TextInstructions = LineParser.ParseLiteral(ref input, "INSTRUCTIONS");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case CaptchaType.ReCaptchaV2:
                    // SOLVECAPTCHA ReCaptchaV2 "sitekey" "siteurl" [IsInvisible?]
                    SiteKey = LineParser.ParseLiteral(ref input, "SITE KEY");
                    SiteUrl = LineParser.ParseLiteral(ref input, "SITE URL");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case CaptchaType.ReCaptchaV3:
                    // SOLVECAPTCHA ReCaptchaV3 "sitekey" "siteurl" "action" "minscore"
                    SiteKey = LineParser.ParseLiteral(ref input, "SITE KEY");
                    SiteUrl = LineParser.ParseLiteral(ref input, "SITE URL");
                    Action = LineParser.ParseLiteral(ref input, "ACTION");
                    MinScore = LineParser.ParseLiteral(ref input, "MIN SCORE");
                    break;

                case CaptchaType.FunCaptcha:
                    // SOLVECAPTCHA FunCaptcha "pkey" "serviceurl" [NoJS?]
                    SiteUrl = LineParser.ParseLiteral(ref input, "SITE URL");
                    PublicKey = LineParser.ParseLiteral(ref input, "PUBLIC KEY");
                    ServiceUrl = LineParser.ParseLiteral(ref input, "SERVICE URL");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case CaptchaType.KeyCaptcha:
                    // SOLVECAPTCHA KeyCaptcha "userid" "sessionid" "wss1" "wss2"
                    SiteUrl = LineParser.ParseLiteral(ref input, "SITE URL");
                    UserId = LineParser.ParseLiteral(ref input, "USER ID");
                    SessionId = LineParser.ParseLiteral(ref input, "SESSION ID");
                    WebServerSign1 = LineParser.ParseLiteral(ref input, "WEBSERVER SIGN 1");
                    WebServerSign2 = LineParser.ParseLiteral(ref input, "WEBSERVER SIGN 2");
                    break;

                case CaptchaType.HCaptcha:
                    //  SOLVECAPTCHA HCaptcha "sitekey" "siteurl"
                    SiteKey = LineParser.ParseLiteral(ref input, "SITE KEY");
                    SiteUrl = LineParser.ParseLiteral(ref input, "SITE URL");
                    break;

                case CaptchaType.GeeTest:
                    // SOLVECAPTCHA GeeTest "siteurl" "gt" "challenge" "apiserver"
                    SiteUrl = LineParser.ParseLiteral(ref input, "SITE URL");
                    GT = LineParser.ParseLiteral(ref input, "GT");
                    Challenge = LineParser.ParseLiteral(ref input, "CHALLENGE");
                    ApiServer = LineParser.ParseLiteral(ref input, "API SERVER");
                    break;

                case CaptchaType.Capy:
                    // SOLVECAPTCHA Capy "sitekey" "siteurl"
                    SiteKey = LineParser.ParseLiteral(ref input, "SITE KEY");
                    SiteUrl = LineParser.ParseLiteral(ref input, "SITE URL");
                    break;
            }

            while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                LineParser.SetBool(ref input, this);

            if (LineParser.Lookahead(ref input) == TokenType.Literal)
                UserAgent = LineParser.ParseLiteral(ref input, "USER AGENT");

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("SOLVECAPTCHA")
                .Token(Type);

            switch (Type)
            {
                case CaptchaType.TextCaptcha:
                    writer
                        .Literal(Question)
                        .Token(LanguageGroup)
                        .Token(Language);
                    break;

                case CaptchaType.ImageCaptcha:
                    writer
                        .Literal(Base64)
                        .Token(LanguageGroup)
                        .Token(Language)
                        .Integer(MinLength)
                        .Integer(MaxLength)
                        .Token(CharSet)
                        .Literal(TextInstructions)
                        .Boolean(IsPhrase, nameof(IsPhrase))
                        .Boolean(CaseSensitive, nameof(CaseSensitive))
                        .Boolean(RequiresCalculation, nameof(RequiresCalculation));
                    break;

                case CaptchaType.ReCaptchaV2:
                    writer
                        .Literal(SiteKey)
                        .Literal(SiteUrl)
                        .Boolean(IsInvisible, nameof(IsInvisible));
                    break;

                case CaptchaType.ReCaptchaV3:
                    writer
                        .Literal(SiteKey)
                        .Literal(SiteUrl)
                        .Literal(Action)
                        .Literal(MinScore);
                    break;

                case CaptchaType.FunCaptcha:
                    writer
                        .Literal(SiteUrl)
                        .Literal(PublicKey)
                        .Literal(ServiceUrl)
                        .Boolean(NoJS, nameof(NoJS));
                    break;

                case CaptchaType.KeyCaptcha:
                    writer
                        .Literal(SiteUrl)
                        .Literal(UserId)
                        .Literal(SessionId)
                        .Literal(WebServerSign1)
                        .Literal(WebServerSign2);
                    break;

                case CaptchaType.Capy:
                case CaptchaType.HCaptcha:
                    writer
                        .Literal(SiteKey)
                        .Literal(SiteUrl);
                    break;

                case CaptchaType.GeeTest:
                    writer
                        .Literal(SiteUrl)
                        .Literal(GT)
                        .Literal(Challenge)
                        .Literal(ApiServer);
                    break;
            }

            writer
                .Boolean(UseProxy, nameof(UseProxy))
                .Literal(UserAgent, nameof(UserAgent));

            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            var service = Captchas.GetService(data.GlobalSettings.Captchas);
            string errorMessage;

            Proxy proxy = data.UseProxies && UseProxy
                ? proxy = new Proxy {
                    Host = data.Proxy.Host,
                    Port = int.Parse(data.Proxy.Port),
                    Type = (ProxyType)Enum.Parse(typeof(ProxyType), data.Proxy.Type.ToString()),
                    Username = data.Proxy.Username,
                    Password = data.Proxy.Password,
                    UserAgent = UserAgent,
                    Cookies = data.Cookies.ToList().Concat(data.GlobalCookies.ToList()).Select(p => (p.Key, p.Value)).ToArray() }
                : null;

            if (!data.GlobalSettings.Captchas.BypassBalanceCheck)
            {
                try
                {
                    try
                    {
                        data.Balance = service.GetBalanceAsync().Result;
                        data.Log($"[{data.GlobalSettings.Captchas.CurrentService}] Balance: ${data.Balance}");

                        if (data.Balance < (decimal)0.002)
                            throw new Exception("The remaining balance is too low!");
                    }
                    catch (Exception ex) // This unwraps aggregate exceptions
                    {
                        if (ex is AggregateException) throw ex.InnerException;
                        else throw;
                    }
                }
                catch (BadAuthenticationException ex) 
                { 
                    data.Log(new LogEntry($"Bad credentials! {ex.Message}", Colors.Tomato));
                    return;
                }
                catch (Exception ex) 
                {
                    data.Log(new LogEntry($"An error occurred! {ex.Message}", Colors.Tomato));
                }
            }

            try
            {
                try
                {
                    var response = GetResponse(service, data, proxy);

                    InsertVariable(data, false, response.Id.ToString(), "CAPTCHAID");

                    switch (response)
                    {
                        case StringResponse r:
                            InsertVariable(data, false, r.Response, "SOLUTION");
                            data.Log(new LogEntry($"Captcha solved successfully! Id: {r.Id} Solution: {r.Response}", Colors.GreenYellow));
                            break;

                        case GeeTestResponse r:
                            InsertVariable(data, false, r.Challenge, "GT_CHALLENGE");
                            InsertVariable(data, false, r.Validate, "GT_VALIDATE");
                            InsertVariable(data, false, r.SecCode, "GT_SECCODE");
                            data.Log(new LogEntry($"Captcha solved successfully! Id: {r.Id} Challenge: {r.Challenge}\r\nValidate: {r.Validate}\r\nSecCode: {r.SecCode}", Colors.GreenYellow));
                            break;
                    }

                    return;
                }
                catch (Exception ex) // This unwraps aggregate exceptions
                {
                    if (ex is AggregateException) throw ex.InnerException;
                    else throw;
                }
            }
            catch (NotSupportedException ex) { errorMessage = $"The currently selected service ({data.GlobalSettings.Captchas.CurrentService}) does not support this task! {ex.Message}"; }
            catch (TaskCreationException ex) { errorMessage = $"Could not create the captcha task! {ex.Message}"; }
            catch (TaskSolutionException ex) { errorMessage = $"Could not solve the captcha! {ex.Message}"; }
            catch (Exception ex)             { errorMessage = $"An error occurred! {ex.Message}"; }

            data.Log(new LogEntry(errorMessage, Colors.Tomato));
        }

        private CaptchaResponse GetResponse(CaptchaService service, BotData data, Proxy proxy)
        {
            CaptchaResponse response;

            switch (Type)
            {
                case CaptchaType.TextCaptcha:
                    response = service.SolveTextCaptchaAsync(ReplaceValues(Question, data), new TextCaptchaOptions
                    { CaptchaLanguage = Language, CaptchaLanguageGroup = LanguageGroup }).Result;
                    break;

                case CaptchaType.ImageCaptcha:
                    response = service.SolveImageCaptchaAsync(ReplaceValues(Base64, data), new ImageCaptchaOptions
                    {
                        CaptchaLanguage = Language,
                        CaptchaLanguageGroup = LanguageGroup,
                        IsPhrase = IsPhrase,
                        CaseSensitive = CaseSensitive,
                        RequiresCalculation = RequiresCalculation,
                        CharacterSet = CharSet,
                        MinLength = MinLength,
                        MaxLength = MaxLength,
                        TextInstructions = ReplaceValues(TextInstructions, data)
                    }).Result;
                    break;

                case CaptchaType.ReCaptchaV2:
                    response = service.SolveRecaptchaV2Async(ReplaceValues(SiteKey, data), ReplaceValues(SiteUrl, data),
                        IsInvisible, proxy).Result;
                    break;

                case CaptchaType.ReCaptchaV3:
                    response = service.SolveRecaptchaV3Async(ReplaceValues(SiteKey, data), ReplaceValues(SiteUrl, data),
                        ReplaceValues(Action, data), float.Parse(ReplaceValues(MinScore, data)), proxy).Result;
                    break;

                case CaptchaType.FunCaptcha:
                    response = service.SolveFuncaptchaAsync(ReplaceValues(PublicKey, data), ReplaceValues(ServiceUrl, data),
                        ReplaceValues(SiteUrl, data), NoJS, proxy).Result;
                    break;

                case CaptchaType.HCaptcha:
                    response = service.SolveHCaptchaAsync(ReplaceValues(SiteKey, data), ReplaceValues(SiteUrl, data), proxy).Result;
                    break;

                case CaptchaType.Capy:
                    response = service.SolveCapyAsync(ReplaceValues(SiteKey, data), ReplaceValues(SiteUrl, data), proxy).Result;
                    break;

                case CaptchaType.KeyCaptcha:
                    response = service.SolveKeyCaptchaAsync(ReplaceValues(UserId, data), ReplaceValues(SessionId, data),
                        ReplaceValues(WebServerSign1, data), ReplaceValues(WebServerSign2, data), ReplaceValues(SiteUrl, data), proxy).Result;
                    break;

                case CaptchaType.GeeTest:
                    response = service.SolveGeeTestAsync(ReplaceValues(GT, data), ReplaceValues(Challenge, data),
                        ReplaceValues(ApiServer, data), ReplaceValues(SiteUrl, data), proxy).Result;
                    break;

                default:
                    throw new NotSupportedException();
            }

            return response;
        }
    }
}
