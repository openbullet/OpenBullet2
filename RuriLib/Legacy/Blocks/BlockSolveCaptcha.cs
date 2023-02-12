using System;
using System.Linq;
using CaptchaSharp.Enums;
using CaptchaSharp.Exceptions;
using CaptchaSharp.Models;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using System.Threading.Tasks;
using RuriLib.Logging;
using RuriLib.Models.Captchas;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that solves a captcha challenge using a remote solving service.
    /// </summary>
    public class BlockSolveCaptcha : BlockBase
    {
        #region General
        /// <summary>The type of captcha to solve.</summary>
        public CaptchaType Type { get; set; } = CaptchaType.ReCaptchaV2;

        /// <summary>Whether to tell the service to use your proxy to solve captchas.</summary>
        public bool UseProxy { get; set; } = false;

        /// <summary>The user agent that the service will use (should be the same as yours).</summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36";
        #endregion

        #region TextCaptcha
        /// <summary>The captcha question to answer.</summary>
        public string Question { get; set; } = "";

        /// <summary>The language group of the captcha text.</summary>
        public CaptchaLanguageGroup LanguageGroup { get; set; } = CaptchaLanguageGroup.NotSpecified;

        /// <summary>The language group of the captcha text.</summary>
        public CaptchaLanguage Language { get; set; } = CaptchaLanguage.NotSpecified;
        #endregion

        #region ImageCaptcha
        /// <summary>The captcha image as a base64 encoded string.</summary>
        public string Base64 { get; set; } = "";

        /// <summary>Whether the captcha has multiple words.</summary>
        public bool IsPhrase { get; set; } = false;

        /// <summary>Whether the captcha solution should be case sensitive.</summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>The type of characters that can appear in the image.</summary>
        public CharacterSet CharSet { get; set; } = CharacterSet.NotSpecified;

        /// <summary>Whether the captcha requires mathematical calculations.</summary>
        public bool RequiresCalculation { get; set; } = false;

        /// <summary>The minimum length of the solution (0 is unspecified).</summary>
        public int MinLength { get; set; } = 0;

        /// <summary>The maximum length of the solution (0 is unspecified).</summary>
        public int MaxLength { get; set; } = 0;

        /// <summary>Any additional instructions useful to the solution.</summary>
        public string TextInstructions { get; set; } = "";
        #endregion

        #region Token Captchas
        /// <summary>The site key.</summary>
        public string SiteKey { get; set; } = "";

        /// <summary>The site URL.</summary>
        public string SiteUrl { get; set; } = "";

        /// <summary>Whether the ReCaptchaV2 is invisible.</summary>
        public bool IsInvisible { get; set; } = false;

        /// <summary>The ReCaptchaV3 action.</summary>
        public string Action { get; set; } = "";

        /// <summary>The ReCaptchaV3 minimum required score.</summary>
        public string MinScore { get; set; } = "0.3";
        #endregion

        #region FunCaptcha
        /// <summary>The public key of the website.</summary>
        public string PublicKey { get; set; } = "";

        /// <summary>The service URL of the website.</summary>
        public string ServiceUrl { get; set; } = "";

        /// <summary>Whether to solve the FunCaptcha with JS disabled.</summary>
        public bool NoJS { get; set; } = false;
        #endregion

        #region KeyCaptcha
        /// <summary>The user ID.</summary>
        public string UserId { get; set; } = "";

        /// <summary>The session ID.</summary>
        public string SessionId { get; set; } = "";

        /// <summary>The WebServerSign1.</summary>
        public string WebServerSign1 { get; set; } = "";

        /// <summary>The WebServerSign2.</summary>
        public string WebServerSign2 { get; set; } = "";
        #endregion

        #region GeeTest
        /// <summary>The gt static key.</summary>
        public string GT { get; set; } = "";

        /// <summary>The challenge dynamic key.</summary>
        public string Challenge { get; set; } = "";

        /// <summary>The api server domain.</summary>
        public string ApiServer { get; set; } = "";
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
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            string errorMessage;

            Proxy proxy = data.UseProxy && UseProxy
                ? proxy = new Proxy
                {
                    Host = data.Proxy.Host,
                    Port = data.Proxy.Port,
                    Type = (ProxyType)Enum.Parse(typeof(ProxyType), data.Proxy.Type.ToString()),
                    Username = data.Proxy.Username,
                    Password = data.Proxy.Password,
                    UserAgent = UserAgent,
                    Cookies = data.COOKIES.ToList().Concat(ls.GlobalCookies.ToList()).Select(p => (p.Key, p.Value)).ToArray()
                }
                : null;

            var provider = data.Providers.Captcha;

            if (provider.CheckBalanceBeforeSolving)
            {
                try
                {
                    try
                    {
                        data.CaptchaCredit = await provider.GetBalanceAsync();
                        data.Logger.Log($"[{provider.ServiceType}] Balance: ${data.CaptchaCredit}");

                        if (data.CaptchaCredit < (decimal)0.002)
                        {
                            throw new Exception("The remaining balance is too low!");
                        }
                    }
                    catch (Exception ex) // This unwraps aggregate exceptions
                    {
                        if (ex is AggregateException)
                        {
                            throw ex.InnerException;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                catch (BadAuthenticationException ex)
                {
                    data.Logger.Log($"Bad credentials! {ex.Message}", LogColors.Tomato);
                    return;
                }
                catch (Exception ex)
                {
                    data.Logger.Log($"An error occurred! {ex.Message}", LogColors.Tomato);
                }
            }

            try
            {
                try
                {
                    var response = await GetResponse(ls, proxy);

                    InsertVariable(ls, false, response.Id.ToString(), "CAPTCHAID");

                    switch (response)
                    {
                        case StringResponse r:
                            InsertVariable(ls, false, r.Response, "SOLUTION");
                            data.Logger.Log($"Captcha solved successfully! Id: {r.IdString} Solution: {r.Response}", LogColors.GreenYellow);
                            break;

                        case GeeTestResponse r:
                            InsertVariable(ls, false, r.Challenge, "GT_CHALLENGE");
                            InsertVariable(ls, false, r.Validate, "GT_VALIDATE");
                            InsertVariable(ls, false, r.SecCode, "GT_SECCODE");
                            data.Logger.Log($"Captcha solved successfully! Id: {r.IdString} Challenge: {r.Challenge}\r\nValidate: {r.Validate}\r\nSecCode: {r.SecCode}", LogColors.GreenYellow);
                            break;
                    }

                    // Save the captcha id for reporting
                    data.SetObject("lastCaptchaInfo", new CaptchaInfo { Id = response.IdString, Type = Type });

                    return;
                }
                catch (Exception ex) // This unwraps aggregate exceptions
                {
                    if (ex is AggregateException)
                    {
                        throw ex.InnerException;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (NotSupportedException ex)
            {
                errorMessage = $"The currently selected service ({provider.ServiceType}) does not support this task! {ex.Message}";
            }
            catch (TaskCreationException ex)
            {
                errorMessage = $"Could not create the captcha task! {ex.Message}";
            }
            catch (TaskSolutionException ex)
            {
                errorMessage = $"Could not solve the captcha! {ex.Message}";
            }
            catch (Exception ex)
            {
                errorMessage = $"An error occurred! {ex.Message}";
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                data.Logger.Log(errorMessage, LogColors.Tomato);
            }
        }

        private async Task<CaptchaResponse> GetResponse(LSGlobals ls, Proxy proxy)
        {
            var provider = ls.BotData.Providers.Captcha;

            return Type switch
            {
                CaptchaType.TextCaptcha => await provider.SolveTextCaptchaAsync(ReplaceValues(Question, ls), new TextCaptchaOptions
                    { CaptchaLanguage = Language, CaptchaLanguageGroup = LanguageGroup }),

                CaptchaType.ImageCaptcha => await provider.SolveImageCaptchaAsync(ReplaceValues(Base64, ls), new ImageCaptchaOptions
                {
                    CaptchaLanguage = Language,
                    CaptchaLanguageGroup = LanguageGroup,
                    IsPhrase = IsPhrase,
                    CaseSensitive = CaseSensitive,
                    RequiresCalculation = RequiresCalculation,
                    CharacterSet = CharSet,
                    MinLength = MinLength,
                    MaxLength = MaxLength,
                    TextInstructions = ReplaceValues(TextInstructions, ls)
                }),

                CaptchaType.ReCaptchaV2 => await provider.SolveRecaptchaV2Async(ReplaceValues(SiteKey, ls), ReplaceValues(SiteUrl, ls),
                    "", false, IsInvisible, proxy),

                CaptchaType.ReCaptchaV3 => await provider.SolveRecaptchaV3Async(ReplaceValues(SiteKey, ls), ReplaceValues(SiteUrl, ls),
                    ReplaceValues(Action, ls), float.Parse(ReplaceValues(MinScore, ls)), false, proxy),

                CaptchaType.FunCaptcha => await provider.SolveFuncaptchaAsync(ReplaceValues(PublicKey, ls), ReplaceValues(ServiceUrl, ls),
                    ReplaceValues(SiteUrl, ls), NoJS, proxy),

                CaptchaType.HCaptcha => await provider.SolveHCaptchaAsync(ReplaceValues(SiteKey, ls), ReplaceValues(SiteUrl, ls), proxy),

                CaptchaType.Capy => await provider.SolveCapyAsync(ReplaceValues(SiteKey, ls), ReplaceValues(SiteUrl, ls), proxy),

                CaptchaType.KeyCaptcha => await provider.SolveKeyCaptchaAsync(ReplaceValues(UserId, ls), ReplaceValues(SessionId, ls),
                    ReplaceValues(WebServerSign1, ls), ReplaceValues(WebServerSign2, ls), ReplaceValues(SiteUrl, ls), proxy),

                CaptchaType.GeeTest => await provider.SolveGeeTestAsync(ReplaceValues(GT, ls), ReplaceValues(Challenge, ls),
                    ReplaceValues(ApiServer, ls), ReplaceValues(SiteUrl, ls), proxy),

                _ => throw new NotSupportedException(),
            };
        }
    }
}
