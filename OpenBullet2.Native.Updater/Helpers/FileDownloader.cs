using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OpenBullet2.Native.Updater.Helpers;

public class FileDownloader
{
    public static async Task<MemoryStream> DownloadAsync(HttpClient client, string url,
        IProgress<double> progress)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
            
        // We need to specify the Accept header as application/octet-stream
        // to get the raw file instead of the json response
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var content = response.Content;
        var total = response.Content.Headers.ContentLength!;
        var downloaded = 0.0;

        var memoryStream = new MemoryStream();
        await using var stream = await content.ReadAsStreamAsync();

        var buffer = new byte[81920];
        var isMoreToRead = true;

        do
        {
            var read = await stream.ReadAsync(buffer);
            if (read == 0)
            {
                isMoreToRead = false;
            }
            else
            {
                await memoryStream.WriteAsync(buffer.AsMemory(0, read));
                downloaded += read;
                progress.Report(downloaded / total.Value * 100);
            }
        } while (isMoreToRead);
            
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
