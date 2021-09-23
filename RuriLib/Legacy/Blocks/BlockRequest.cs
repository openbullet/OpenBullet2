using Extreme.Net;
using RuriLib.Functions.Formats;
using RuriLib.LS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using RuriLib.Functions.Requests;
using RuriLib.Functions.Files;
using MultipartContent = RuriLib.Functions.Requests.MultipartContent;

namespace RuriLib
{
    /// <summary>
    /// The types of request that can be performed.
    /// </summary>
    public enum RequestType
    {
        /// <summary>A standard request with standard content.</summary>
        Standard,

        /// <summary>A request which uses the 'Authentication: Basic' header.</summary>
        BasicAuth,

        /// <summary>A request which contains multipart content (strings and/or files).</summary>
        Multipart,

        /// <summary>A request which sends a raw byte stream.</summary>
        Raw
    }

    /// <summary>
    /// The available types of multipart contents.
    /// </summary>
    public enum MultipartContentType
    {
        /// <summary>A string content.</summary>
        String,

        /// <summary>A file content.</summary>
        File
    }

    /// <summary>
    /// The type of data expected inside the HTTP response.
    /// </summary>
    public enum ResponseType
    {
        /// <summary>A string response, e.g. an HTML page.</summary>
        String,

        /// <summary>A file response, e.g. an image.</summary>
        File,

        /// <summary>A byte array response encoded as a base64 string.</summary>
        Base64String
    }

    /// <summary>
    /// A block that can perform HTTP requests.
    /// </summary>
    public class BlockRequest : BlockBase
    {
        #region Variables
        private string url = "https://google.com";
        /// <summary>The URL to call, including additional GET query parameters.</summary>
        public string Url { get { return url; } set { url = value; OnPropertyChanged(); } }

        private RequestType requestType = RequestType.Standard;
        /// <summary>The request type.</summary>
        public RequestType RequestType { get { return requestType; } set { requestType = value; OnPropertyChanged(); } }

        // Basic Auth
        private string authUser = "";
        /// <summary>The username for basic auth requests.</summary>
        public string AuthUser { get { return authUser; } set { authUser = value; OnPropertyChanged(); } }

        private string authPass = "";
        /// <summary>The password for basic auth requests.</summary>
        public string AuthPass { get { return authPass; } set { authPass = value; OnPropertyChanged(); } }

        // Standard
        private string postData = "";
        /// <summary>The content of the request, sent after the headers. Use '\n' to input a linebreak.</summary>
        public string PostData { get { return postData; } set { postData = value; OnPropertyChanged(); } }

        // Raw
        private string rawData = "";
        /// <summary>The content of the request as a raw HEX string that will be sent as a bytestream.</summary>
        public string RawData { get { return rawData; } set { rawData = value; OnPropertyChanged(); } }

        private HttpMethod method = HttpMethod.GET;
        /// <summary>The method of the HTTP request.</summary>
        public HttpMethod Method { get { return method; } set { method = value; OnPropertyChanged(); } }

        private SecurityProtocol securityProtocol = SecurityProtocol.SystemDefault;
        /// <summary>The security protocol(s) to use for the HTTPS request.</summary>
        public SecurityProtocol SecurityProtocol { get { return securityProtocol; } set { securityProtocol = value; OnPropertyChanged(); } }

        /// <summary>The custom headers that are sent in the HTTP request.</summary>
        public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>() {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36" },
            { "Pragma", "no-cache" },
            { "Accept", "*/*" }
        };

        /// <summary>The custom cookies that are sent in the HTTP request.</summary>
        public Dictionary<string, string> CustomCookies { get; set; } = new Dictionary<string, string>() { };

        private string contentType = "application/x-www-form-urlencoded";
        /// <summary>The type of content the server should expect.</summary>
        public string ContentType { get { return contentType; } set { contentType = value; OnPropertyChanged(); } }

        private bool autoRedirect = true;
        /// <summary>Whether to perform automatic redirection in the case of 3xx headers.</summary>
        public bool AutoRedirect { get { return autoRedirect; } set { autoRedirect = value; OnPropertyChanged(); } }

        private bool readResponseSource = true;
        /// <summary>Whether to read the stream of data from the HTTP response. Set to false if only the headers are needed, in order to speed up the process.</summary>
        public bool ReadResponseSource { get { return readResponseSource; } set { readResponseSource = value; OnPropertyChanged(); } }

        private bool encodeContent = false;
        /// <summary>Whether to URL encode the content before sending it.</summary>
        public bool EncodeContent { get { return encodeContent; } set { encodeContent = value; OnPropertyChanged(); } }

        private bool acceptEncoding = true;
        /// <summary>Whether to automatically generate an Accept-Encoding header.</summary>
        public bool AcceptEncoding { get { return acceptEncoding; } set { acceptEncoding = value; OnPropertyChanged(); } }

        // Multipart
        private string multipartBoundary = "";
        /// <summary>The boundary that separates multipart contents.</summary>
        public string MultipartBoundary { get { return multipartBoundary; } set { multipartBoundary = value; OnPropertyChanged(); } }

        /// <summary>The list of contents to send in a multipart request.</summary>
        public List<MultipartContent> MultipartContents { get; set; } = new List<MultipartContent>();

        private ResponseType responseType = ResponseType.String;
        /// <summary>The type of response expected from the server.</summary>
        public ResponseType ResponseType { get { return responseType; } set { responseType = value; OnPropertyChanged(); } }

        private string downloadPath = "";
        /// <summary>The path of the file where a FILE response needs to be stored.</summary>
        public string DownloadPath { get { return downloadPath; } set { downloadPath = value; OnPropertyChanged(); } }

        private string outputVariable = "";
        /// <summary>The variable name for Base64String response.</summary>
        public string OutputVariable { get { return outputVariable; } set { outputVariable = value; OnPropertyChanged(); } }
        
        private bool saveAsScreenshot = false;
        /// <summary>Whether to add the downloaded image to the default screenshot path.</summary>
        public bool SaveAsScreenshot { get { return saveAsScreenshot; } set { saveAsScreenshot = value; OnPropertyChanged(); } }
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
        {
            return input.Split(new[] { separator }, count).Select(s => s.Trim()).ToArray();
        }

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
                    if (HttpRequest.CanContainRequestBody(method))
                    {
                        writer
                            .Token("CONTENT")
                            .Literal(PostData)
                            .Indent()
                            .Token("CONTENTTYPE")
                            .Literal(ContentType);
                    }
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
                    if (HttpRequest.CanContainRequestBody(method))
                    {
                        writer
                            .Token("RAWDATA")
                            .Literal(RawData)
                            .Indent()
                            .Token("CONTENTTYPE")
                            .Literal(ContentType);
                    }
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
        public override void Process(BotData data)
        {
            base.Process(data);

            // Setup
            var request = new Request();
            request.Setup(data.GlobalSettings, securityProtocol, AutoRedirect, data.ConfigSettings.MaxRedirects, AcceptEncoding);

            var localUrl = ReplaceValues(Url, data);
            data.Log(new LogEntry($"Calling URL: {localUrl}", Colors.MediumTurquoise));
            
            // Set content
            switch (RequestType)
            {
                case RequestType.Standard:
                    request.SetStandardContent(ReplaceValues(PostData, data), ReplaceValues(ContentType, data), Method, EncodeContent, GetLogBuffer(data));
                    break;

                case RequestType.BasicAuth:
                    request.SetBasicAuth(ReplaceValues(AuthUser, data), ReplaceValues(AuthPass, data));
                    break;

                case RequestType.Multipart:
                    var contents = MultipartContents.Select(m =>
                        new MultipartContent()
                        {
                            Name = ReplaceValues(m.Name, data),
                            Value = ReplaceValues(m.Value, data),
                            ContentType = ReplaceValues(m.Value, data),
                            Type = m.Type
                        });
                    request.SetMultipartContent(contents, ReplaceValues(MultipartBoundary, data), GetLogBuffer(data));
                    break;

                case RequestType.Raw:
                    request.SetRawContent(ReplaceValues(RawData, data), ReplaceValues(ContentType, data), Method, GetLogBuffer(data));
                    break;
            }

            // Set proxy
            if (data.UseProxies)
            {
                request.SetProxy(data.Proxy);
            }

            // Set headers
            data.Log(new LogEntry("Sent Headers:", Colors.DarkTurquoise));
            var headers = CustomHeaders.Select( h =>
                    new KeyValuePair<string, string> (ReplaceValues(h.Key, data), ReplaceValues(h.Value, data))
                ).ToDictionary(h => h.Key, h => h.Value);
            request.SetHeaders(headers, AcceptEncoding, GetLogBuffer(data));

            // Set cookies
            data.Log(new LogEntry("Sent Cookies:", Colors.MediumTurquoise));

            foreach (var cookie in CustomCookies) // Add new user-defined custom cookies to the bot's cookie jar
                data.Cookies[ReplaceValues(cookie.Key, data)] = ReplaceValues(cookie.Value, data);

            request.SetCookies(data.Cookies, GetLogBuffer(data));

            // End the request part
            data.LogNewLine();

            // Perform the request
            try
            {
                (data.Address, data.ResponseCode, data.ResponseHeaders, data.Cookies) = request.Perform(localUrl, Method, GetLogBuffer(data));
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

            // Save the response content
            switch (ResponseType)
            {
                case ResponseType.String:
                    data.ResponseSource = request.SaveString(ReadResponseSource, data.ResponseHeaders, GetLogBuffer(data));
                    break;

                case ResponseType.File:
                    if (SaveAsScreenshot)
                    {
                        Files.SaveScreenshot(request.GetResponseStream(), data); // Read the stream
                        data.Log(new LogEntry("File saved as screenshot", Colors.Green));
                    }
                    else
                    {
                        request.SaveFile(ReplaceValues(DownloadPath, data), GetLogBuffer(data));
                    }
                    break;

                case ResponseType.Base64String:
                    var base64 = Convert.ToBase64String(request.GetResponseStream().ToArray());
                    InsertVariable(data, false, base64, OutputVariable);
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
            StringBuilder sb = new StringBuilder();
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
            StringBuilder sb = new StringBuilder();
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
            StringBuilder sb = new StringBuilder();
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

        private List<LogEntry> GetLogBuffer(BotData data) => data.GlobalSettings.General.EnableBotLog || data.IsDebug ? data.LogBuffer : null;
    }
}
