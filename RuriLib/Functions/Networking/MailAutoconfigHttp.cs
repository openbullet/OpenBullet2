using RuriLib.Exceptions;
using RuriLib.Functions.Http;
using RuriLib.Http.Models;
using RuriLib.Models.Bots;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RuriLib.Functions.Networking;

internal static class MailAutoconfigHttp
{
    public static async Task<string> GetStringAsync(BotData data, string url)
    {
        using var httpClient = HttpFactory.GetRLHttpClient(data.UseProxy ? data.Proxy : null, new HttpOptions
        {
            ConnectTimeout = TimeSpan.FromMilliseconds(30000),
            ReadWriteTimeout = TimeSpan.FromMilliseconds(30000)
        });

        using var request = new HttpRequest { Uri = new Uri(url) };
        request.Headers["Accept"] = "application/xml, text/xml;q=0.9, */*;q=0.8";

        using var response = await httpClient.SendAsync(request, data.CancellationToken).ConfigureAwait(false);

        if (response.StatusCode is < HttpStatusCode.OK or >= HttpStatusCode.MultipleChoices)
        {
            throw new BlockExecutionException(
                $"The autoconfig request to {url} failed with HTTP {(int)response.StatusCode} ({response.StatusCode})");
        }

        var content = response.Content
                      ?? throw new BlockExecutionException("The autoconfig response content is not available");

        return await content.ReadAsStringAsync(data.CancellationToken).ConfigureAwait(false);
    }
}
