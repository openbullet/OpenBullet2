using RuriLib.Functions.Http.Options;
using RuriLib.Models.Bots;
using System.Threading.Tasks;

namespace RuriLib.Functions.Http
{
    internal interface IHttpRequestHandler
    {
        Task HttpRequestStandard(BotData data, StandardHttpRequestOptions options);
        Task HttpRequestRaw(BotData data, RawHttpRequestOptions options);
        Task HttpRequestBasicAuth(BotData data, BasicAuthHttpRequestOptions options);
        Task HttpRequestMultipart(BotData data, MultipartHttpRequestOptions options);
    }
}
