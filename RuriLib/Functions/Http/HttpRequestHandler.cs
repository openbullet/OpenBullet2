using RuriLib.Functions.Http.Options;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Functions.Http
{
    internal abstract class HttpRequestHandler
    {
        protected static readonly string[] commaHeaders = new[] { "Accept", "Accept-Encoding" };

        public virtual Task HttpRequestStandard(BotData data, StandardHttpRequestOptions options)
            => throw new NotImplementedException();
        public virtual Task HttpRequestRaw(BotData data, RawHttpRequestOptions options)
            => throw new NotImplementedException();
        public virtual Task HttpRequestBasicAuth(BotData data, BasicAuthHttpRequestOptions options)
            => throw new NotImplementedException();
        public virtual Task HttpRequestMultipart(BotData data, MultipartHttpRequestOptions options)
            => throw new NotImplementedException();

        /// <summary>
        /// Generates a random string to be used for boundary.
        /// </summary>
        protected static string GenerateMultipartBoundary()
        {
            var builder = new StringBuilder();
            var random = new Random();
            for (var i = 0; i < 16; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return $"------WebKitFormBoundary{builder.ToString().ToLower()}";
        }

        protected static StreamContent CreateFileContent(Stream stream, string fieldName, string fileName, string contentType)
        {
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = $"\"{fieldName}\"",
                FileName = $"\"{fileName}\""
            }; // the extra quotes are key here
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return fileContent;
        }

        protected static string GetMediaHeaderString(string contentType)
            => new MediaTypeHeaderValue(contentType).ToString();

        protected static string SerializeMultipart(string boundary, List<MyHttpContent> contents)
        {
            using var writer = new StringWriter();

            foreach (var content in contents)
            {
                writer.WriteLine(boundary);

                switch (content)
                {
                    case StringHttpContent x:
                        writer.WriteLine($"Content-Disposition: form-data; name={x.Name}");
                        writer.WriteLine($"Content-Type: {GetMediaHeaderString(x.ContentType)}");
                        writer.WriteLine();
                        writer.WriteLine(x.Data);
                        break;

                    case RawHttpContent x:
                        writer.WriteLine($"Content-Disposition: form-data; name={x.Name}");
                        writer.WriteLine($"Content-Type: {GetMediaHeaderString(x.ContentType)}");
                        writer.WriteLine();
                        writer.WriteLine(Encoding.UTF8.GetString(x.Data));
                        break;

                    case FileHttpContent x:
                        writer.WriteLine($"Content-Disposition: form-data; name=\"{x.Name}\"; filename=\"{Path.GetFileName(x.FileName)}\"");
                        writer.WriteLine($"Content-Type: {GetMediaHeaderString(x.ContentType)}");
                        writer.WriteLine();
                        writer.WriteLine("[FILE CONTENTS NOT LOGGED]");
                        break;
                }
            }

            writer.WriteLine(boundary);

            return writer.ToString();
        }

        protected static TlsCipherSuite[] ParseCipherSuites(List<string> cipherSuites)
        {
            if (cipherSuites == null)
            {
                return Array.Empty<TlsCipherSuite>();
            }

            var parsed = new List<TlsCipherSuite>();

            foreach (var suite in cipherSuites)
            {
                try
                {
                    parsed.Add(Enum.Parse<TlsCipherSuite>(suite));
                }
                catch
                {
                    throw new NotSupportedException($"Cipher suite not supported: {suite}");
                }
            }

            return parsed.ToArray();
        }

        protected static HttpOptions GetClientOptions(BotData data, Options.HttpRequestOptions options) => new()
        {
            ConnectTimeout = data.Providers.ProxySettings.ConnectTimeout,
            ReadWriteTimeout = data.Providers.ProxySettings.ReadWriteTimeout,
            AutoRedirect = options.AutoRedirect,
            MaxNumberOfRedirects = options.MaxNumberOfRedirects,
            SecurityProtocol = options.SecurityProtocol,
            UseCustomCipherSuites = options.UseCustomCipherSuites,
            CustomCipherSuites = ParseCipherSuites(options.CustomCipherSuites),
            CertRevocationMode = data.Providers.Security.X509RevocationMode,
            ReadResponseContent = options.ReadResponseContent
        };
    }
}
