using RuriLib.Functions.Http;
using RuriLib.Models.Blocks.Parameters;
using System.Collections.Generic;

namespace RuriLib.Models.Blocks.Custom
{
    public class HttpRequestBlockDescriptor : BlockDescriptor
    {
        public HttpRequestBlockDescriptor()
        {
            Id = "HttpRequest";
            Name = Id;
            Description = "Performs an Http request and reads the response";
            Category = new BlockCategory
            {
                Name = "Http",
                BackgroundColor = "#32cd32",
                ForegroundColor = "#000",
                Path = "RuriLib.Blocks.Requests.Http",
                Namespace = "RuriLib.Blocks.Requests.Http.Methods",
                Description = "Blocks for performing Http requests"
            };
            Parameters = new Dictionary<string, BlockParameter>
            {
                { "url", new StringParameter { Name = "url", DefaultValue = "https://google.com" } },
                { "method", new EnumParameter { Name = "method", EnumType = typeof(HttpMethod), DefaultValue = HttpMethod.GET.ToString() } },
                { "autoRedirect", new BoolParameter { Name = "autoRedirect", DefaultValue = true } },
                { "securityProtocol", new EnumParameter { Name = "securityProtocol",
                    EnumType = typeof(SecurityProtocol), DefaultValue = SecurityProtocol.SystemDefault.ToString() } },
                { "useCustomCipherSuites", new BoolParameter { Name = "useCustomCipherSuites", DefaultValue = false } },
                { "customCipherSuites", new ListOfStringsParameter { Name = "customCipherSuites",
                    InputMode = Settings.SettingInputMode.Fixed, DefaultValue = new List<string>
                    {
                        "TLS_AES_128_GCM_SHA256",
                        "TLS_CHACHA20_POLY1305_SHA256",
                        "TLS_AES_256_GCM_SHA384",
                        "TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256",
                        "TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256",
                        "TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256",
                        "TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256",
                        "TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384",
                        "TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384",
                        "TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA",
                        "TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA",
                        "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA",
                        "TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA",
                        "TLS_RSA_WITH_AES_128_GCM_SHA256",
                        "TLS_RSA_WITH_AES_256_GCM_SHA384",
                        "TLS_RSA_WITH_AES_128_CBC_SHA",
                        "TLS_RSA_WITH_AES_256_CBC_SHA",
                        "TLS_RSA_WITH_3DES_EDE_CBC_SHA"
                    }
                }},
                { "customCookies", new DictionaryOfStringsParameter { Name = "customCookies", InputMode = Settings.SettingInputMode.Interpolated,
                    DefaultValue = new Dictionary<string, string>() } },
                { "customHeaders", new DictionaryOfStringsParameter { Name = "customHeaders", InputMode = Settings.SettingInputMode.Interpolated,
                    DefaultValue = new Dictionary<string, string>
                    {
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36" },
                        { "Pragma", "no-cache" },
                        { "Accept", "*/*" },
                        { "Accept-Language", "en-US,en;q=0.8" }
                    }
                }},
                { "timeoutMilliseconds", new IntParameter { Name = "timeoutMilliseconds", DefaultValue = 10000 } },
                { "httpVersion", new StringParameter { Name = "httpVersion", DefaultValue = "1.1" } }
            };
        }
    }
}
