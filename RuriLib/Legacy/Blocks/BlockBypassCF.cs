using RuriLib.LS;
using System;
using System.Windows.Media;
using System.Net;
using CloudflareSolverRe;
using CloudflareSolverRe.Types;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using RuriLib.Functions.Requests;
using CaptchaSharp;
using CaptchaSharp.Services;
using RuriLib.Functions.Captchas;

namespace RuriLib
{
    /// <summary>
    /// A block that can bypass Cloudflare protections.
    /// </summary>
    public class BlockBypassCF : BlockBase
    {
        private string url = "";
        /// <summary>The URL of the Cloudflare-protected website.</summary>
        public string Url { get { return url; } set { url = value; OnPropertyChanged(); } }

        private string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36";
        /// <summary>The User-Agent header to use when solving the challenge.</summary>
        public string UserAgent { get { return userAgent; } set { userAgent = value; OnPropertyChanged(); } }

        private bool printResponseInfo = true;
        /// <summary>Whether to print the full response info to the log.</summary>
        public bool PrintResponseInfo { get { return printResponseInfo; } set { printResponseInfo = value; OnPropertyChanged(); } }

        private bool autoRedirect = false;
        /// <summary>Whether to enable auto-redirect (situational, depends on site).</summary>
        public bool AutoRedirect { get { return autoRedirect; } set { autoRedirect = value; OnPropertyChanged(); } }

        private SecurityProtocol securityProtocol = SecurityProtocol.SystemDefault;
        /// <summary>The security protocol(s) to use for the HTTPS request.</summary>
        public SecurityProtocol SecurityProtocol { get { return securityProtocol; } set { securityProtocol = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates a Cloudflare bypass block.
        /// </summary>
        public BlockBypassCF()
        {
            Label = "BYPASS CF";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            /*
             * Syntax
             * BYPASSCF "URL" SECPROTO PROTOCOL ["UA"]
             * */

            Url = LineParser.ParseLiteral(ref input, "URL");

            if (input != "" && LineParser.Lookahead(ref input) == TokenType.Literal)
            {
                UserAgent = LineParser.ParseLiteral(ref input, "UA");
            }

            if (input != "" && LineParser.ParseToken(ref input, TokenType.Parameter, false, false) == "SECPROTO")
            {
                LineParser.ParseToken(ref input, TokenType.Parameter, true);
                SecurityProtocol = LineParser.ParseEnum(ref input, "Security Protocol", typeof(SecurityProtocol));
            }

            while (input != "")
            {
                LineParser.SetBool(ref input, this);
            }

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("BYPASSCF")
                .Literal(Url)
                .Literal(UserAgent, nameof(UserAgent));

            if (SecurityProtocol != SecurityProtocol.SystemDefault)
            {
                writer
                    .Token("SECPROTO")
                    .Token(SecurityProtocol);
            }
                
            writer
                .Boolean(PrintResponseInfo, nameof(PrintResponseInfo))
                .Boolean(AutoRedirect, nameof(AutoRedirect));
            return writer.ToString();
        }

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            // If the clearance info is already set and we're not getting it fresh each time, skip
            if (data.UseProxies)
            {
                if (data.Proxy.Clearance != "" && !data.GlobalSettings.Proxies.AlwaysGetClearance)
                {
                    data.Log(new LogEntry("Skipping CF Bypass because there is already a valid cookie", Colors.White));
                    data.Cookies["cf_clearance"] = data.Proxy.Clearance;
                    data.Cookies["__cfduid"] = data.Proxy.Cfduid;
                    return;
                }
            }

            var localUrl = ReplaceValues(url, data);
            var uri = new Uri(localUrl);

            // Initialize the captcha provider
            CaptchaService service = Captchas.GetService(data.GlobalSettings.Captchas);

            // Initialize the Cloudflare Solver
            CloudflareSolver cf = new CloudflareSolver(service, ReplaceValues(UserAgent, data))
            {
                ClearanceDelay = 3000,
                MaxCaptchaTries = 1,
                MaxTries = 3
            };

            // Create the cookie container
            CookieContainer cookies = new CookieContainer();
            foreach (var cookie in data.Cookies)
            {
                cookies.Add(new Cookie(cookie.Key, cookie.Value, "/", uri.Host));
            }

            // Initialize the http handler
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = AutoRedirect,
                CookieContainer = cookies,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                SslProtocols = SecurityProtocol.ToSslProtocols()
            };

            // Assign the proxy to the inner handler if necessary
            if (data.UseProxies)
            {
                if (data.Proxy.Type != Extreme.Net.ProxyType.Http)
                {
                    throw new Exception($"The proxy type {data.Proxy.Type} is not supported by this block yet");
                }

                handler.Proxy = new WebProxy(data.Proxy.Proxy, false);
                handler.UseProxy = true;

                if (!string.IsNullOrEmpty(data.Proxy.Username))
                {
                    handler.Proxy.Credentials = new NetworkCredential(data.Proxy.Username, data.Proxy.Password);
                }
            }

            // Initialize the http client
            HttpClient http = new HttpClient(handler);
            http.Timeout = TimeSpan.FromSeconds(data.GlobalSettings.General.RequestTimeout);
            http.DefaultRequestHeaders.Add("User-Agent", ReplaceValues(UserAgent, data));

            SolveResult result = new SolveResult();

            try
            {
                result = cf.Solve(http, handler, uri, ReplaceValues(UserAgent, data)).Result;
            }
            catch (AggregateException ex)
            {
                // Join all the aggregate exception inner exception messages
                var message = string.Join(Environment.NewLine, ex.InnerExceptions.Select(e => e.Message));
                
                if (data.ConfigSettings.IgnoreResponseErrors)
                {
                    data.Log(new LogEntry(message, Colors.Tomato));
                    data.ResponseCode = message;
                    return;
                }
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                if (data.ConfigSettings.IgnoreResponseErrors)
                {
                    data.Log(new LogEntry(ex.Message, Colors.Tomato));
                    data.ResponseSource = ex.Message;
                    return;
                }
                throw;
            }

            if (result.Success)
            {
                data.Log(new LogEntry($"[Success] Protection bypassed: {result.DetectResult.Protection}", Colors.GreenYellow));
            }
            else if (result.DetectResult.Protection == CloudflareProtection.Unknown)
            {
                data.Log(new LogEntry($"Unknown protection, skipping the bypass!", Colors.Tomato));
            }
            else
            {
                var message = $"CF Bypass Failed: {result.FailReason}";

                if (data.ConfigSettings.IgnoreResponseErrors)
                {
                    data.Log(new LogEntry(message, Colors.Tomato));
                    data.ResponseSource = message;
                    return;
                }
                throw new Exception(message);
            }

            // Now that we got the cookies, proceed with the normal request
            HttpResponseMessage response = null;
            try
            {
                response = http.GetAsync(uri).Result;
            }
            catch (Exception ex)
            {
                if (data.ConfigSettings.IgnoreResponseErrors)
                {
                    data.ResponseSource = ex.Message;
                    return;
                }
                throw new Exception(ex.Message);
            }
            finally
            {
                handler.Dispose();
                http.Dispose();
            }

            var responseString = response.Content.ReadAsStringAsync().Result;

            // Save the cloudflare cookies
            var clearance = "";
            var cfduid = "";
            foreach (Cookie cookie in cookies.GetCookies(uri))
            {
                switch (cookie.Name)
                {
                    case "cf_clearance":
                        clearance = cookie.Value;
                        break;

                    case "__cfduid":
                        cfduid = cookie.Value;
                        break;
                }
            }

            // Save the cookies in the proxy
            if (data.UseProxies)
            {
                data.Proxy.Clearance = clearance;
                data.Proxy.Cfduid = cfduid;
            }

            if (clearance != "")
            {
                data.Log(new LogEntry("Got Cloudflare clearance!", Colors.GreenYellow));
                data.Log(new LogEntry(clearance + Environment.NewLine + cfduid + Environment.NewLine, Colors.White));
            }

            // Get address
            data.Address = response.RequestMessage.RequestUri.AbsoluteUri;
            if (PrintResponseInfo) data.Log(new LogEntry($"Address: {data.Address}", Colors.Cyan));

            // Get code
            data.ResponseCode = ((int)response.StatusCode).ToString();
            if (PrintResponseInfo) data.Log(new LogEntry($"Response code: {data.ResponseCode}", Colors.Cyan));

            // Get headers
            if (PrintResponseInfo) data.Log(new LogEntry("Received headers:", Colors.DeepPink));
            data.ResponseHeaders.Clear();
            foreach (var header in response.Headers)
            {
                var h = new KeyValuePair<string, string>(header.Key, header.Value.First());
                data.ResponseHeaders.Add(h.Key, h.Value);
                if (PrintResponseInfo) data.Log(new LogEntry($"{h.Key}: {h.Value}", Colors.LightPink));
            }

            // Add the Content-Length header if it was not sent by the server
            if (!data.ResponseHeaders.ContainsKey("Content-Length"))
            {
                if (data.ResponseHeaders.ContainsKey("Content-Encoding") && data.ResponseHeaders["Content-Encoding"].Contains("gzip"))
                {
                    data.ResponseHeaders["Content-Length"] = GZip.Zip(responseString).Length.ToString();
                }
                else
                {
                    data.ResponseHeaders["Content-Length"] = responseString.Length.ToString();
                }

                if (PrintResponseInfo) data.Log(new LogEntry($"Content-Length: {data.ResponseHeaders["Content-Length"]}", Colors.LightPink));
            }

            // Get cookies
            if (PrintResponseInfo) data.Log(new LogEntry("Received cookies:", Colors.Goldenrod));
            foreach (Cookie cookie in cookies.GetCookies(uri))
            {
                data.Cookies[cookie.Name] = cookie.Value;
                if (PrintResponseInfo) data.Log(new LogEntry($"{cookie.Name}: {cookie.Value}", Colors.LightGoldenrodYellow));
            }

            // Print source
            data.ResponseSource = responseString;
            if (PrintResponseInfo)
            {
                data.Log(new LogEntry("Response Source:", Colors.Green));
                data.Log(new LogEntry(data.ResponseSource, Colors.GreenYellow));
            }
        }
    }
}
