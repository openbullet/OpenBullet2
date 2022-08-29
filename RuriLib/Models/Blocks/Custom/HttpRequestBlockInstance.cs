using RuriLib.Exceptions;
using RuriLib.Extensions;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Custom.HttpRequest;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Blocks.Custom
{
    public class HttpRequestBlockInstance : BlockInstance
    {
        public RequestParams RequestParams { get; set; } = new StandardRequestParams();

        public bool Safe { get; set; } = false;

        public HttpRequestBlockInstance(HttpRequestBlockDescriptor descriptor)
            : base(descriptor)
        {
            
        }

        public override string ToLC(bool printDefaultParams = false)
        {
            /*
             *   TYPE:STANDARD
             *   "name=hello&value=hi"
             *   "application/x-www-form-urlencoded"
             *   
             *   TYPE:RAW
             *   BASE64_DATA
             *   "application/octet-stream"
             *   
             *   TYPE:BASICAUTH
             *   "myUser"
             *   "myPass"
             *   
             *   TYPE:MULTIPART
             *   "myBoundary"
             *   CONTENT:STRING "name" "content" "content-type" 
             *   CONTENT:RAW "name" BASE64_DATA "content-type"
             *   CONTENT:FILE "name" "fileName" "content-type"
             *   
             */

            using var writer = new LoliCodeWriter(base.ToLC(printDefaultParams));

            if (Safe)
            {
                writer.AppendLine("SAFE", 2);
            }

            switch (RequestParams)
            {
                case StandardRequestParams x:
                    writer
                        .AppendLine("TYPE:STANDARD", 2)
                        .AppendLine(LoliCodeWriter.GetSettingValue(x.Content), 2)
                        .AppendLine(LoliCodeWriter.GetSettingValue(x.ContentType), 2);
                    break;

                case RawRequestParams x:
                    writer
                        .AppendLine("TYPE:RAW", 2)
                        .AppendLine(LoliCodeWriter.GetSettingValue(x.Content), 2)
                        .AppendLine(LoliCodeWriter.GetSettingValue(x.ContentType), 2);
                    break;

                case BasicAuthRequestParams x:
                    writer
                        .AppendLine("TYPE:BASICAUTH", 2)
                        .AppendLine(LoliCodeWriter.GetSettingValue(x.Username), 2)
                        .AppendLine(LoliCodeWriter.GetSettingValue(x.Password), 2);
                    break;

                case MultipartRequestParams x:
                    writer
                        .AppendLine("TYPE:MULTIPART", 2)
                        .AppendLine(LoliCodeWriter.GetSettingValue(x.Boundary), 2);

                    foreach (var content in x.Contents)
                    {
                        switch (content)
                        {
                            case StringHttpContentSettingsGroup y:
                                writer
                                    .AppendToken("CONTENT:STRING", 2)
                                    .AppendToken(LoliCodeWriter.GetSettingValue(y.Name))
                                    .AppendToken(LoliCodeWriter.GetSettingValue(y.Data))
                                    .AppendLine(LoliCodeWriter.GetSettingValue(y.ContentType));
                                break;

                            case RawHttpContentSettingsGroup y:
                                writer
                                    .AppendToken("CONTENT:RAW", 2)
                                    .AppendToken(LoliCodeWriter.GetSettingValue(y.Name))
                                    .AppendToken(LoliCodeWriter.GetSettingValue(y.Data))
                                    .AppendLine(LoliCodeWriter.GetSettingValue(y.ContentType));
                                break;

                            case FileHttpContentSettingsGroup y:
                                writer
                                    .AppendToken("CONTENT:FILE", 2)
                                    .AppendToken(LoliCodeWriter.GetSettingValue(y.Name))
                                    .AppendToken(LoliCodeWriter.GetSettingValue(y.FileName))
                                    .AppendLine(LoliCodeWriter.GetSettingValue(y.ContentType));
                                break;
                        }
                    }

                    break;
            }

            return writer.ToString();
        }

        public override void FromLC(ref string script, ref int lineNumber)
        {
            /*
             *   TYPE:STANDARD
             *   "name=hello&value=hi"
             *   "application/x-www-form-urlencoded"
             *   
             *   TYPE:RAW
             *   BASE64_DATA
             *   "application/octet-stream"
             *   
             *   TYPE:BASICAUTH
             *   "myUser"
             *   "myPass"
             *   
             *   TYPE:MULTIPART
             *   "myBoundary"
             *   CONTENT:STRING "name" "content" "content-type" 
             *   CONTENT:RAW "name" BASE64_DATA "content-type"
             *   CONTENT:FILE "name" "fileName" "content-type"
             *   
             */

            // First parse the options that are common to every BlockInstance
            base.FromLC(ref script, ref lineNumber);

            using var reader = new StringReader(script);

            while (reader.ReadLine() is { } line)
            {
                line = line.Trim();
                var lineCopy = line;
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("SAFE"))
                {
                    Safe = true;
                    continue;
                }

                if (line.StartsWith("TYPE:"))
                {
                    try
                    {
                        var reqParams = Regex.Match(line, "TYPE:([A-Z]+)").Groups[1].Value;

                        switch (reqParams)
                        {
                            case "STANDARD":
                                var standardReqParams = new StandardRequestParams();

                                // Read one line to parse the content
                                line = reader.ReadLine().Trim();
                                lineCopy = line;
                                lineNumber++;
                                LoliCodeParser.ParseSettingValue(ref line, standardReqParams.Content, new StringParameter());

                                // Read another line to parse the content-type
                                line = reader.ReadLine().Trim();
                                lineCopy = line;
                                lineNumber++;
                                LoliCodeParser.ParseSettingValue(ref line, standardReqParams.ContentType, new StringParameter());

                                RequestParams = standardReqParams;
                                break;

                            case "RAW":
                                var rawReqParams = new RawRequestParams();

                                // Read one line to parse the content
                                line = reader.ReadLine().Trim();
                                lineCopy = line;
                                lineNumber++;
                                LoliCodeParser.ParseSettingValue(ref line, rawReqParams.Content, new ByteArrayParameter());


                                // Read another line to parse the content-type
                                line = reader.ReadLine().Trim();
                                lineCopy = line;
                                lineNumber++;
                                LoliCodeParser.ParseSettingValue(ref line, rawReqParams.ContentType, new StringParameter());

                                RequestParams = rawReqParams;
                                break;

                            case "BASICAUTH":
                                var basicAuthReqParams = new BasicAuthRequestParams();

                                // Read one line to parse the username
                                line = reader.ReadLine().Trim();
                                lineCopy = line;
                                lineNumber++;
                                LoliCodeParser.ParseSettingValue(ref line, basicAuthReqParams.Username, new StringParameter());

                                // Read another line to parse the password
                                line = reader.ReadLine().Trim();
                                lineCopy = line;
                                lineNumber++;
                                LoliCodeParser.ParseSettingValue(ref line, basicAuthReqParams.Password, new StringParameter());

                                RequestParams = basicAuthReqParams;
                                break;

                            case "MULTIPART":
                                var multipartReqParams = new MultipartRequestParams();

                                // Read one line to parse the boundary
                                line = reader.ReadLine().Trim();
                                lineCopy = line;
                                lineNumber++;
                                LoliCodeParser.ParseSettingValue(ref line, multipartReqParams.Boundary, new StringParameter());

                                RequestParams = multipartReqParams;
                                break;

                            default:
                                throw new LoliCodeParsingException(lineNumber, $"Invalid type: {reqParams}");
                        }
                    }
                    catch (NullReferenceException)
                    {
                        throw new LoliCodeParsingException(lineNumber, "Missing options for the selected content");
                    }
                    catch
                    {
                        throw new LoliCodeParsingException(lineNumber, $"Could not parse the setting: {lineCopy.TruncatePretty(50)}");
                    }
                }

                else if (line.StartsWith("CONTENT:"))
                {
                    try
                    {
                        var multipart = (MultipartRequestParams)RequestParams;
                        var token = LineParser.ParseToken(ref line);
                        var tokenType = Regex.Match(token, "CONTENT:([A-Z]+)").Groups[1].Value;

                        switch (tokenType)
                        {
                            case "STRING":
                                var stringContent = new StringHttpContentSettingsGroup();
                                LoliCodeParser.ParseSettingValue(ref line, stringContent.Name, new StringParameter());
                                LoliCodeParser.ParseSettingValue(ref line, stringContent.Data, new StringParameter());
                                LoliCodeParser.ParseSettingValue(ref line, stringContent.ContentType, new StringParameter());
                                multipart.Contents.Add(stringContent);
                                break;

                            case "RAW":
                                var rawContent = new RawHttpContentSettingsGroup();
                                LoliCodeParser.ParseSettingValue(ref line, rawContent.Name, new StringParameter());
                                LoliCodeParser.ParseSettingValue(ref line, rawContent.Data, new ByteArrayParameter());
                                LoliCodeParser.ParseSettingValue(ref line, rawContent.ContentType, new StringParameter());
                                multipart.Contents.Add(rawContent);
                                break;

                            case "FILE":
                                var fileContent = new FileHttpContentSettingsGroup();
                                LoliCodeParser.ParseSettingValue(ref line, fileContent.Name, new StringParameter());
                                LoliCodeParser.ParseSettingValue(ref line, fileContent.FileName, new StringParameter());
                                LoliCodeParser.ParseSettingValue(ref line, fileContent.ContentType, new StringParameter());
                                multipart.Contents.Add(fileContent);
                                break;
                        }
                    }
                    catch
                    {
                        throw new LoliCodeParsingException(lineNumber, $"Could not parse the multipart content: {lineCopy.TruncatePretty(50)}");
                    }
                }

                else
                {
                    try
                    {
                        LoliCodeParser.ParseSetting(ref line, Settings, Descriptor);
                    }
                    catch
                    {
                        throw new LoliCodeParsingException(lineNumber, $"Could not parse the setting: {lineCopy.TruncatePretty(50)}");
                    }
                }
            }
        }

        public override string ToCSharp(List<string> definedVariables, ConfigSettings settings)
        {
            using var writer = new StringWriter();

            if (Safe)
            {
                writer.WriteLine("try {");
            }

            writer.Write("await ");

            switch (RequestParams)
            {
                case StandardRequestParams x:
                    writer.Write("HttpRequestStandard(data, new StandardHttpRequestOptions { ");
                    writer.Write("Content = " + CSharpWriter.FromSetting(x.Content) + ", ");
                    writer.Write("ContentType = " + CSharpWriter.FromSetting(x.ContentType) + ", ");
                    writer.Write("UrlEncodeContent = " + GetSettingValue("urlEncodeContent") + ", ");
                    break;

                case RawRequestParams x:
                    writer.Write("HttpRequestRaw(data, new RawHttpRequestOptions { ");
                    writer.Write("Content = " + CSharpWriter.FromSetting(x.Content) + ", ");
                    writer.Write("ContentType = " + CSharpWriter.FromSetting(x.ContentType) + ", ");
                    break;

                case BasicAuthRequestParams x:
                    writer.Write("HttpRequestBasicAuth(data, new BasicAuthHttpRequestOptions { ");
                    writer.Write("Username = " + CSharpWriter.FromSetting(x.Username) + ", ");
                    writer.Write("Password = " + CSharpWriter.FromSetting(x.Password) + ", ");
                    break;

                case MultipartRequestParams x:
                    writer.Write("HttpRequestMultipart(data, new MultipartHttpRequestOptions { ");
                    writer.Write("Boundary = " + CSharpWriter.FromSetting(x.Boundary) + ", ");
                    writer.Write("Contents = " + SerializeMultipart(x.Contents) + ", ");
                    break;
            }

            writer.Write("Url = " + GetSettingValue("url") + ", ");
            writer.Write("Method = " + GetSettingValue("method") + ", ");
            writer.Write("AutoRedirect = " + GetSettingValue("autoRedirect") + ", ");
            writer.Write("MaxNumberOfRedirects = " + GetSettingValue("maxNumberOfRedirects") + ", ");
            writer.Write("ReadResponseContent = " + GetSettingValue("readResponseContent") + ", ");
            writer.Write("AbsoluteUriInFirstLine = " + GetSettingValue("absoluteUriInFirstLine") + ", ");
            writer.Write("HttpLibrary = " + GetSettingValue("httpLibrary") + ", ");
            writer.Write("SecurityProtocol = " + GetSettingValue("securityProtocol") + ", ");
            writer.Write("CustomCookies = " + GetSettingValue("customCookies") + ", ");
            writer.Write("CustomHeaders = " + GetSettingValue("customHeaders") + ", ");
            writer.Write("TimeoutMilliseconds = " + GetSettingValue("timeoutMilliseconds") + ", ");
            writer.Write("HttpVersion = " + GetSettingValue("httpVersion") + ", ");
            writer.Write("CodePagesEncoding = " + GetSettingValue("codePagesEncoding") + ", ");
            writer.Write("AlwaysSendContent = " + GetSettingValue("alwaysSendContent") + ", ");
            writer.Write("DecodeHtml = " + GetSettingValue("decodeHtml") + ", ");
            writer.Write("UseCustomCipherSuites = " + GetSettingValue("useCustomCipherSuites") + ", ");
            writer.Write("CustomCipherSuites = " + GetSettingValue("customCipherSuites") + " ");

            writer.WriteLine("}).ConfigureAwait(false);");

            if (Safe)
            {
                writer.WriteLine("} catch (Exception safeException) {");
                writer.WriteLine("data.ERROR = safeException.PrettyPrint();");
                writer.WriteLine("data.Logger.Log($\"[SAFE MODE] Exception caught and saved to data.ERROR: {data.ERROR}\", LogColors.Tomato); }");
            }

            return writer.ToString();
        }

        private string SerializeMultipart(List<HttpContentSettingsGroup> contents)
            => $"new List<MyHttpContent> {{ {string.Join(", ", contents.Select(c => SerializeContent(c)))} }}";

        private string SerializeContent(HttpContentSettingsGroup content)
        {
            return content switch
            {
                StringHttpContentSettingsGroup x =>
                    $"new StringHttpContent({CSharpWriter.FromSetting(x.Name)}, {CSharpWriter.FromSetting(x.Data)}, {CSharpWriter.FromSetting(x.ContentType)})",
                RawHttpContentSettingsGroup x =>
                    $"new RawHttpContent({CSharpWriter.FromSetting(x.Name)}, {CSharpWriter.FromSetting(x.Data)}, {CSharpWriter.FromSetting(x.ContentType)})",
                FileHttpContentSettingsGroup x =>
                    $"new FileHttpContent({CSharpWriter.FromSetting(x.Name)}, {CSharpWriter.FromSetting(x.FileName)}, {CSharpWriter.FromSetting(x.ContentType)})",
                _ => throw new NotImplementedException()
            };
        }

        private string GetSettingValue(string name)
            => CSharpWriter.FromSetting(Settings[name]);
    }
}
