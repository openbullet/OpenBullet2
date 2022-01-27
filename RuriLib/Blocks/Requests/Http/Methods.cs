using RuriLib.Attributes;
using RuriLib.Functions.Http;
using RuriLib.Functions.Http.Options;
using RuriLib.Models.Bots;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Requests.Http
{
    [BlockCategory("Http", "Blocks for performing Http requests", "#32cd32")]
    public static class Methods
    {
        /*
         * These are not blocks, but they take BotData as an input. The HttpRequestBlockInstance will take care
         * of writing C# code that calls these methods where necessary once it's transpiled.
         */

        // STANDARD REQUESTS
        // This method is accessed via a custom descriptor so it must not be an auto block
        public static Task HttpRequestStandard(BotData data, StandardHttpRequestOptions options)
            => GetHandler(options).HttpRequestStandard(data, options);

        // RAW REQUESTS
        // This method is accessed via a custom descriptor so it must not be an auto block
        public static Task HttpRequestRaw(BotData data, RawHttpRequestOptions options)
            => GetHandler(options).HttpRequestRaw(data, options);

        // BASIC AUTH
        // This method is accessed via a custom descriptor so it must not be an auto block
        public static Task HttpRequestBasicAuth(BotData data, BasicAuthHttpRequestOptions options)
            => GetHandler(options).HttpRequestBasicAuth(data, options);

        // MULTIPART
        // This method is accessed via a custom descriptor so it must not be an auto block
        public static Task HttpRequestMultipart(BotData data, MultipartHttpRequestOptions options)
            => GetHandler(options).HttpRequestMultipart(data, options);

        private static HttpRequestHandler GetHandler(HttpRequestOptions options)
            => options.HttpLibrary switch
            {
                HttpLibrary.RuriLibHttp => new RLHttpClientRequestHandler(),
                HttpLibrary.SystemNet => new HttpClientRequestHandler(),
                _ => throw new System.NotImplementedException()
            };
    }
}
