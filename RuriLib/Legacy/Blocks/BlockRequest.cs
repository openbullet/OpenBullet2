using RuriLib.Functions.Http;
using RuriLib.Legacy.LS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RuriLib.Blocks.Requests.Http;
using System.Threading.Tasks;
using RuriLib.Legacy.Models;
using RuriLib.Functions.Http.Options;
using RuriLib.Functions.Conversion;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Logging;
using System.IO;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that can perform HTTP requests.
    /// </summary>
    public class BlockRequest : BlockBase
    {
        #region Variables
        /// <summary>The URL to call, including additional GET query parameters.</summary>
        public string Url { get; set; } = "https://google.com";

        /// <summary>The request type.</summary>
        public RequestType RequestType { get; set; } = RequestType.Standard;

        // Basic Auth
        /// <summary>The username for basic auth requests.</summary>
        public string AuthUser { get; set; } = "";

        /// <summary>The password for basic auth requests.</summary>
        public string AuthPass { get; set; } = "";

        // Standard
        /// <summary>The content of the request, sent after the headers. Use '\n' to input a linebreak.</summary>
        public string PostData { get; set; } = "";

        // Raw
        /// <summary>The content of the request as a raw HEX string that will be sent as a bytestream.</summary>
        public string RawData { get; set; } = "";

        /// <summary>The method of the HTTP request.</summary>
        public HttpMethod Method { get; set; } = HttpMethod.GET;

        /// <summary>The security protocol(s) to use for the HTTPS request.</summary>
        public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SystemDefault;

        /// <summary>The custom headers that are sent in the HTTP request.</summary>
        public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>() {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36" },
            { "Pragma", "no-cache" },
            { "Accept", "*/*" }
        };

        /// <summary>The custom cookies that are sent in the HTTP request.</summary>
        public Dictionary<string, string> CustomCookies { get; set; } = new();

        /// <summary>The type of content the server should expect.</summary>
        public string ContentType { get; set; } = "application/x-www-form-urlencoded";

        /// <summary>Whether to perform automatic redirection in the case of 3xx headers.</summary>
        public bool AutoRedirect { get; set; } = true;

        /// <summary>Whether to read the stream of data from the HTTP response. Set to false if only the headers are needed, in order to speed up the process.</summary>
        public bool ReadResponseSource { get; set; } = true;

        /// <summary>Whether to URL encode the content before sending it.</summary>
        public bool EncodeContent { get; set; } = false;

        /// <summary>Whether to automatically generate an Accept-Encoding header.</summary>
        public bool AcceptEncoding { get; set; } = true;

        // Multipart
        /// <summary>The boundary that separates multipart contents.</summary>
        public string MultipartBoundary { get; set; } = "";

        /// <summary>The list of contents to send in a multipart request.</summary>
        public List<MultipartContent> MultipartContents { get; set; } = new();

        /// <summary>The type of response expected from the server.</summary>
        public ResponseType ResponseType { get; set; } = ResponseType.String;

        /// <summary>The path of the file where a FILE response needs to be stored.</summary>
        public string DownloadPath { get; set; } = "";

        /// <summary>The variable name for Base64String response.</summary>
        public string OutputVariable { get; set; } = "";

        /// <summary>Whether to add the downloaded image to the default screenshot path.</summary>
        public bool SaveAsScreenshot { get; set; } = false;
        #endregion

        /// <summary>
        /// Creates a Request block.
        /// </summary>
        public BlockRequest()
        {
            Label = "REQUEST";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            Method = (HttpMethod)LineParser.ParseEnum(ref input, "METHOD", typeof(HttpMethod));
            Url = LineParser.ParseLiteral(ref input, "URL");

            while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                LineParser.SetBool(ref input, this);

            CustomHeaders.Clear(); // Remove the default headers

            while (input != string.Empty && !input.StartsWith("->"))
            {
                var parsed = LineParser.ParseToken(ref input, TokenType.Parameter, true).ToUpper();
                switch (parsed)
                {
                    case "MULTIPART":
                        RequestType = RequestType.Multipart;
                        break;

                    case "BASICAUTH":
                        RequestType = RequestType.BasicAuth;
                        break;

                    case "STANDARD":
                        RequestType = RequestType.Standard;
                        break;

                    case "RAW":
                        RequestType = RequestType.Raw;
                        break;

                    case "CONTENT":
                        PostData = LineParser.ParseLiteral(ref input, "POST DATA");
                        break;

                    case "RAWDATA":
                        RawData = LineParser.ParseLiteral(ref input, "RAW DATA");
                        break;

                    case "STRINGCONTENT":
                        var stringContentPair = ParseString(LineParser.ParseLiteral(ref input, "STRING CONTENT"), ':', 2);
                        MultipartContents.Add(new MultipartContent() { Type = MultipartContentType.String, Name = stringContentPair[0], Value = stringContentPair[1] });
                        break;

                    case "FILECONTENT":
                        var fileContentTriplet = ParseString(LineParser.ParseLiteral(ref input, "FILE CONTENT"), ':', 3);
                        MultipartContents.Add(new MultipartContent() { Type = MultipartContentType.File, Name = fileContentTriplet[0], Value = fileContentTriplet[1], ContentType = fileContentTriplet[2] });
                        break;

                    case "COOKIE":
                        var cookiePair = ParseString(LineParser.ParseLiteral(ref input, "COOKIE VALUE"), ':', 2);
                        CustomCookies[cookiePair[0]] = cookiePair[1];
                        break;

                    case "HEADER":
                        var headerPair = ParseString(LineParser.ParseLiteral(ref input, "HEADER VALUE"), ':', 2);
                        CustomHeaders[headerPair[0]] = headerPair[1];
                        break;

                    case "CONTENTTYPE":
                        ContentType = LineParser.ParseLiteral(ref input, "CONTENT TYPE");
                        break;

                    case "USERNAME":
                        AuthUser = LineParser.ParseLiteral(ref input, "USERNAME");
                        break;

                    case "PASSWORD":
                        AuthPass = LineParser.ParseLiteral(ref input, "PASSWORD");
                        break;

                    case "BOUNDARY":
                        MultipartBoundary = LineParser.ParseLiteral(ref input, "BOUNDARY");
                        break;

                    case "SECPROTO":
                        SecurityProtocol = LineParser.ParseEnum(ref input, "Security Protocol", typeof(SecurityProtocol));
                        break;

                    default:
                        break;
                }
            }

            if (input.StartsWith("->"))
            {
                LineParser.EnsureIdentifier(ref input, "->");
                var outType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (outType.ToUpper() == "STRING") ResponseType = ResponseType.String;
                else if (outType.ToUpper() == "FILE")
                {
                    ResponseType = ResponseType.File;
                    DownloadPath = LineParser.ParseLiteral(ref input, "DOWNLOAD PATH");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                    {
                        LineParser.SetBool(ref input, this);
                    }
                }
                else if (outType.ToUpper() == "BASE64")
                {
                    ResponseType = ResponseType.Base64String;
                    OutputVariable = LineParser.ParseLiteral(ref input, "OUTPUT VARIABLE");
                }
            }

            return this;
        }

        /// <summary>
        /// Parses values from a string.
        /// </summary>
        /// <param name="input">The string to parse</param>
        /// <param name="separator">The character that separates the elements</param>
        /// <param name="count">The number of elements to return</param>
        /// <returns>The array of the parsed elements.</returns>
        public static string[] ParseString(string input, char separator, int count)
            => input.Split(new[] { separator }, count).Select(s => s.Trim()).ToArray();

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("REQUEST")
                .Token(Method)
                .Literal(Url)
                .Boolean(AcceptEncoding, "AcceptEncoding")
                .Boolean(AutoRedirect, "AutoRedirect")
                .Boolean(ReadResponseSource, "ReadResponseSource")
                .Boolean(EncodeContent, "EncodeContent")
                .Token(RequestType, "RequestType")
                .Indent();

            switch (RequestType)
            {
                case RequestType.BasicAuth:
                    writer
                        .Token("USERNAME")
                        .Literal(AuthUser)
                        .Token("PASSWORD")
                        .Literal(AuthPass)
                        .Indent();
                    break;

                case RequestType.Standard:
                    writer
                        .Token("CONTENT")
                        .Literal(PostData)
                        .Indent()
                        .Token("CONTENTTYPE")
                        .Literal(ContentType);
                    break;

                case RequestType.Multipart:
                    foreach(var c in MultipartContents)
                    {
                        writer
                            .Indent()
                            .Token($"{c.Type.ToString().ToUpper()}CONTENT");

                        if (c.Type == MultipartContentType.String)
                        {
                            writer.Literal($"{c.Name}: {c.Value}");
                        }
                        else if (c.Type == MultipartContentType.File)
                        {
                            writer.Literal($"{c.Name}: {c.Value}: {c.ContentType}");
                        }
                    }
                    if (!writer.CheckDefault(MultipartBoundary, "MultipartBoundary"))
                    {
                        writer
                            .Indent()
                            .Token("BOUNDARY")
                            .Literal(MultipartBoundary);
                    }
                    break;

                case RequestType.Raw:
                    writer
                        .Token("RAWDATA")
                        .Literal(RawData)
                        .Indent()
                        .Token("CONTENTTYPE")
                        .Literal(ContentType);
                    break;
            }

            if (SecurityProtocol != SecurityProtocol.SystemDefault)
            {
                writer
                    .Indent()
                    .Token("SECPROTO")
                    .Token(SecurityProtocol, "SecurityProtocol");
            }

            foreach (var c in CustomCookies)
            {
                writer
                    .Indent()
                    .Token("COOKIE")
                    .Literal($"{c.Key}: {c.Value}");
            }

            foreach (var h in CustomHeaders)
            {
                writer
                    .Indent()
                    .Token("HEADER")
                    .Literal($"{h.Key}: {h.Value}");
            }

            if (ResponseType == ResponseType.File)
            {
                writer
                    .Indent()
                    .Arrow()
                    .Token("FILE")
                    .Literal(DownloadPath)
                    .Boolean(SaveAsScreenshot, "SaveAsScreenshot");
            }
            else if (ResponseType == ResponseType.Base64String)
            {
                writer
                    .Indent()
                    .Arrow()
                    .Token("BASE64")
                    .Literal(OutputVariable);
            }

            return writer.ToString();
        }

        /// <inheritdoc />
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            var headers = ReplaceValues(CustomHeaders, ls);

            // If no Connection header was specified, add keep-alive by default
            if (!headers.Keys.Any(k => k.Equals("connection", StringComparison.OrdinalIgnoreCase)))
            {
                headers.Add("Connection", "keep-alive");
            }

            // Override the default for Accept-Encoding (default behaviour in OB1)
            foreach (var key in headers.Keys.Where(k => k.Equals("accept-encoding", StringComparison.OrdinalIgnoreCase)).ToArray())
            {
                headers.Remove(key);
            }

            headers["Accept-Encoding"] = "gzip,deflate";

            // Remove Content-Length because it was disregarded in OB1
            var contentLengthHeader = headers.Keys.FirstOrDefault(k => k.Equals("Content-Length", StringComparison.OrdinalIgnoreCase));
            if (contentLengthHeader != null)
            {
                headers.Remove(contentLengthHeader);
            }

            switch (RequestType)
            {
                case RequestType.Standard:
                    var standardOptions = new StandardHttpRequestOptions
                    {
                        AutoRedirect = AutoRedirect,
                        MaxNumberOfRedirects = 8,
                        ReadResponseContent = ReadResponseSource,
                        SecurityProtocol = SecurityProtocol,
                        Content = ReplaceValues(PostData, ls),
                        ContentType = ReplaceValues(ContentType, ls),
                        CustomCookies = ReplaceValues(CustomCookies, ls),
                        CustomHeaders = headers,
                        Method = Method,
                        Url = ReplaceValues(Url, ls),
                        TimeoutMilliseconds = 10000,
                        UrlEncodeContent = EncodeContent
                    };

                    await Methods.HttpRequestStandard(data, standardOptions);
                    break;

                case RequestType.Raw:
                    var rawOptions = new RawHttpRequestOptions
                    {
                        AutoRedirect = AutoRedirect,
                        MaxNumberOfRedirects = 8,
                        ReadResponseContent = ReadResponseSource,
                        SecurityProtocol = SecurityProtocol,
                        Content = HexConverter.ToByteArray(ReplaceValues(RawData, ls)),
                        ContentType = ReplaceValues(ContentType, ls),
                        CustomCookies = ReplaceValues(CustomCookies, ls),
                        CustomHeaders = headers,
                        Method = Method,
                        Url = ReplaceValues(Url, ls),
                        TimeoutMilliseconds = 10000
                    };

                    await Methods.HttpRequestRaw(data, rawOptions);
                    break;

                case RequestType.BasicAuth:
                    var basicAuthOptions = new BasicAuthHttpRequestOptions
                    {
                        AutoRedirect = AutoRedirect,
                        MaxNumberOfRedirects = 8,
                        ReadResponseContent = ReadResponseSource,
                        SecurityProtocol = SecurityProtocol,
                        Username = ReplaceValues(AuthUser, ls),
                        Password = ReplaceValues(AuthPass, ls),
                        CustomCookies = ReplaceValues(CustomCookies, ls),
                        CustomHeaders = headers,
                        Method = Method,
                        Url = ReplaceValues(Url, ls),
                        TimeoutMilliseconds = 10000
                    };

                    await Methods.HttpRequestBasicAuth(data, basicAuthOptions);
                    break;

                case RequestType.Multipart:
                    var multipartOptions = new MultipartHttpRequestOptions
                    {
                        AutoRedirect = AutoRedirect,
                        MaxNumberOfRedirects = 8,
                        ReadResponseContent = ReadResponseSource,
                        SecurityProtocol = SecurityProtocol,
                        Boundary = ReplaceValues(MultipartBoundary, ls),
                        Contents = MultipartContents.Select(mpc => MapMultipartContent(mpc, ls)).ToList(),
                        CustomCookies = ReplaceValues(CustomCookies, ls),
                        CustomHeaders = headers,
                        Method = Method,
                        Url = ReplaceValues(Url, ls),
                        TimeoutMilliseconds = 10000
                    };

                    await Methods.HttpRequestMultipart(data, multipartOptions);
                    break;
            }

            // Save the response content
            switch (ResponseType)
            {
                case ResponseType.File:
                    if (SaveAsScreenshot)
                    {
                        Utils.SaveScreenshot(data.RAWSOURCE, data);
                        data.Logger.Log("File saved as screenshot", LogColors.Green);
                    }
                    else
                    {
                        await File.WriteAllBytesAsync(ReplaceValues(DownloadPath, ls), data.RAWSOURCE).ConfigureAwait(false);
                    }
                    break;

                case ResponseType.Base64String:
                    var base64 = Convert.ToBase64String(data.RAWSOURCE);
                    InsertVariable(ls, false, base64, OutputVariable);
                    break;

                default:
                    break;
            }
        }

        #region Custom Cookies, Headers and Multipart Contents
        /// <summary>
        /// Builds a string containing custom cookies.
        /// </summary>
        /// <returns>One cookie per line, with name and value separated by a colon</returns>
        public string GetCustomCookies()
        {
            var sb = new StringBuilder();
            foreach (var pair in CustomCookies)
            {
                sb.Append($"{pair.Key}: {pair.Value}");
                if (!pair.Equals(CustomCookies.Last())) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sets custom cookies from an array of lines.
        /// </summary>
        /// <param name="lines">The lines containing the colon-separated name and value of the cookies</param>
        public void SetCustomCookies(string[] lines)
        {
            CustomCookies.Clear();
            foreach (var line in lines)
            {
                if (line.Contains(':'))
                {
                    var split = line.Split(new[] { ':' }, 2);
                    CustomCookies[split[0].Trim()] = split[1].Trim();
                }
            }
        }

        /// <summary>
        /// Builds a string containing custom headers.
        /// </summary>
        /// <returns>One header per line, with name and value separated by a colon</returns>
        public string GetCustomHeaders()
        {
            var sb = new StringBuilder();
            foreach (var pair in CustomHeaders)
            {
                sb.Append($"{pair.Key}: {pair.Value}");
                if (!pair.Equals(CustomHeaders.Last())) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sets custom headers from an array of lines.
        /// </summary>
        /// <param name="lines">The lines containing the colon-separated name and value of the headers</param>
        public void SetCustomHeaders(string[] lines)
        {
            CustomHeaders.Clear();
            foreach (var line in lines)
            {
                if (line.Contains(':'))
                {
                    var split = line.Split(new[] { ':' }, 2);
                    CustomHeaders[split[0].Trim()] = split[1].Trim();
                }
            }
        }

        /// <summary>
        /// Builds a string containing multipart content.
        /// </summary>
        /// <returns>One content per line, with type, name and value separated by a colon</returns>
        public string GetMultipartContents()
        {
            var sb = new StringBuilder();
            foreach (var c in MultipartContents)
            {
                sb.Append($"{c.Type.ToString().ToUpper()}: {c.Name}: {c.Value}");
                if (!c.Equals(MultipartContents.Last())) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sets multipart contents from an array of lines.
        /// </summary>
        /// <param name="lines">The lines containing the colon-separated type, name and value of the multipart contents</param>
        public void SetMultipartContents(string[] lines)
        {
            MultipartContents.Clear();
            foreach(var line in lines)
            {
                try
                {
                    var split = line.Split(new[] { ':' }, 3);
                    MultipartContents.Add(new MultipartContent() {
                        Type = (MultipartContentType)Enum.Parse(typeof(MultipartContentType), split[0].Trim(), true),
                        Name = split[1].Trim(),
                        Value = split[2].Trim()
                    });
                }
                catch { }
            }
        }

        #endregion

        private MyHttpContent MapMultipartContent(MultipartContent mpc, LSGlobals ls)
        {
            switch (mpc.Type)
            {
                case MultipartContentType.String:
                    return new StringHttpContent(
                        ReplaceValues(mpc.Name, ls),
                        ReplaceValues(mpc.Value, ls),
                        string.IsNullOrEmpty(mpc.ContentType) ? "text/plain" : ReplaceValues(mpc.ContentType, ls));

                case MultipartContentType.File:
                    return new FileHttpContent(
                        ReplaceValues(mpc.Name, ls),
                        ReplaceValues(mpc.Value, ls),
                        string.IsNullOrEmpty(mpc.ContentType) ? "text/plain" : ReplaceValues(mpc.ContentType, ls));
            }

            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// The types of request that can be performed.
    /// </summary>
    public enum RequestType
    {
        Standard,
        BasicAuth,
        Multipart,
        Raw
    }

    /// <summary>
    /// The available types of multipart contents.
    /// </summary>
    public enum MultipartContentType
    {
        String,
        File
    }

    /// <summary>
    /// The type of data expected inside the HTTP response.
    /// </summary>
    public enum ResponseType
    {
        String,
        File,
        Base64String
    }

    /// <summary>
    /// Represents a Multipart Content
    /// </summary>
    public struct MultipartContent
    {
        public MultipartContentType Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string ContentType { get; set; }
    }
}
