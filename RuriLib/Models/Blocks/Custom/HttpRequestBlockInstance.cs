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

        public HttpRequestBlockInstance(HttpRequestBlockDescriptor descriptor)
        {
            Descriptor = descriptor;
            Id = descriptor.Id;
            Label = descriptor.Name;
            ReadableName = descriptor.Name;

            Settings = Descriptor.Parameters.Select(p => p.ToBlockSetting()).ToList();
        }

        public override string ToLC()
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

            using var writer = new LoliCodeWriter(base.ToLC());

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

        public override void FromLC(ref string script)
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
            base.FromLC(ref script);

            using var reader = new StringReader(script);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim();

                if (line.StartsWith("TYPE:"))
                {
                    var reqParams = Regex.Match(line, "TYPE:([A-Z]+)").Groups[1].Value;

                    switch (reqParams)
                    {
                        case "STANDARD":
                            var standardReqParams = new StandardRequestParams();
                            line = reader.ReadLine();
                            line = line.Trim();
                            LoliCodeParser.ParseSettingValue(ref line, standardReqParams.Content, new StringParameter());
                            line = reader.ReadLine();
                            line = line.Trim();
                            LoliCodeParser.ParseSettingValue(ref line, standardReqParams.ContentType, new StringParameter());
                            RequestParams = standardReqParams;
                            break;

                        case "RAW":
                            var rawReqParams = new RawRequestParams();
                            line = reader.ReadLine();
                            line = line.Trim();
                            LoliCodeParser.ParseSettingValue(ref line, rawReqParams.Content, new ByteArrayParameter());
                            line = reader.ReadLine();
                            line = line.Trim();
                            LoliCodeParser.ParseSettingValue(ref line, rawReqParams.ContentType, new StringParameter());
                            RequestParams = rawReqParams;
                            break;

                        case "BASICAUTH":
                            var basicAuthReqParams = new BasicAuthRequestParams();
                            line = reader.ReadLine();
                            line = line.Trim();
                            LoliCodeParser.ParseSettingValue(ref line, basicAuthReqParams.Username, new StringParameter());
                            line = reader.ReadLine();
                            line = line.Trim();
                            LoliCodeParser.ParseSettingValue(ref line, basicAuthReqParams.Password, new StringParameter());
                            RequestParams = basicAuthReqParams;
                            break;

                        case "MULTIPART":
                            var multipartReqParams = new MultipartRequestParams();
                            line = reader.ReadLine();
                            line = line.Trim();
                            LoliCodeParser.ParseSettingValue(ref line, multipartReqParams.Boundary, new StringParameter());
                            RequestParams = multipartReqParams;
                            break;
                    }
                }

                else if (line.StartsWith("CONTENT:"))
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

                else
                {
                    LoliCodeParser.ParseSetting(ref line, Settings, Descriptor);
                }
            }
        }

        public override string ToCSharp(List<string> definedVariables, ConfigSettings settings)
        {
            using var writer = new StringWriter();
            writer.Write("await ");

            switch (RequestParams)
            {
                case StandardRequestParams x:
                    writer.Write("HttpRequestStandard(data, ");
                    writer.Write(GetSettingValue("url") + ", ");
                    writer.Write(GetSettingValue("method") + ", ");
                    writer.Write(GetSettingValue("autoRedirect") + ", ");
                    writer.Write(GetSettingValue("securityProtocol") + ", ");
                    writer.Write(CSharpWriter.FromSetting(x.Content) + ", ");
                    writer.Write(CSharpWriter.FromSetting(x.ContentType) + ", ");
                    writer.Write(GetSettingValue("customCookies") + ", ");
                    writer.Write(GetSettingValue("customHeaders") + ", ");
                    writer.Write(GetSettingValue("timeoutMilliseconds") + ", ");
                    writer.Write(GetSettingValue("httpVersion") + ", ");
                    writer.Write(GetSettingValue("useCustomCipherSuites") + ", ");
                    writer.Write(GetSettingValue("customCipherSuites"));
                    break;

                case RawRequestParams x:
                    writer.Write("HttpRequestRaw(data, ");
                    writer.Write(GetSettingValue("url") + ", ");
                    writer.Write(GetSettingValue("method") + ", ");
                    writer.Write(GetSettingValue("autoRedirect") + ", ");
                    writer.Write(GetSettingValue("securityProtocol") + ", ");
                    writer.Write(CSharpWriter.FromSetting(x.Content) + ", ");
                    writer.Write(CSharpWriter.FromSetting(x.ContentType) + ", ");
                    writer.Write(GetSettingValue("customCookies") + ", ");
                    writer.Write(GetSettingValue("customHeaders") + ", ");
                    writer.Write(GetSettingValue("timeoutMilliseconds") + ", ");
                    writer.Write(GetSettingValue("httpVersion") + ", ");
                    writer.Write(GetSettingValue("useCustomCipherSuites") + ", ");
                    writer.Write(GetSettingValue("customCipherSuites"));
                    break;

                case BasicAuthRequestParams x:
                    writer.Write("HttpRequestBasicAuth(data, ");
                    writer.Write(GetSettingValue("url") + ", ");
                    writer.Write(GetSettingValue("autoRedirect") + ", ");
                    writer.Write(GetSettingValue("securityProtocol") + ", ");
                    writer.Write(CSharpWriter.FromSetting(x.Username) + ", ");
                    writer.Write(CSharpWriter.FromSetting(x.Password) + ", ");
                    writer.Write(GetSettingValue("customCookies") + ", ");
                    writer.Write(GetSettingValue("customHeaders") + ", ");
                    writer.Write(GetSettingValue("timeoutMilliseconds") + ", ");
                    writer.Write(GetSettingValue("httpVersion") + ", ");
                    writer.Write(GetSettingValue("useCustomCipherSuites") + ", ");
                    writer.Write(GetSettingValue("customCipherSuites"));
                    break;

                case MultipartRequestParams x:
                    writer.Write("HttpRequestMultipart(data, ");
                    writer.Write(GetSettingValue("url") + ", ");
                    writer.Write(GetSettingValue("method") + ", ");
                    writer.Write(GetSettingValue("autoRedirect") + ", ");
                    writer.Write(GetSettingValue("securityProtocol") + ", ");
                    writer.Write(CSharpWriter.FromSetting(x.Boundary) + ", ");
                    writer.Write(SerializeMultipart(x.Contents) + ", ");
                    writer.Write(GetSettingValue("customCookies") + ", ");
                    writer.Write(GetSettingValue("customHeaders") + ", ");
                    writer.Write(GetSettingValue("timeoutMilliseconds") + ", ");
                    writer.Write(GetSettingValue("httpVersion") + ", ");
                    writer.Write(GetSettingValue("useCustomCipherSuites") + ", ");
                    writer.Write(GetSettingValue("customCipherSuites"));
                    break;
            }

            writer.WriteLine(");");

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
            => CSharpWriter.FromSetting(Settings.First(s => s.Name == name));
    }
}
