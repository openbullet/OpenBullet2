using System.Net.Http.Headers;

namespace OpenBullet2.Updater.Core.Helpers;

public class FileDownloader
{
    public static async Task<Stream> DownloadAsync(HttpClient client, string url,
        IProgress<double> progress)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // We need to specify the Accept header as application/octet-stream
        // to get the raw file instead of the json response
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var content = response.Content;
        var total = response.Content.Headers.ContentLength;
        var downloaded = 0.0;

        var tempPath = Path.Combine(Path.GetTempPath(), $"ob2-download-{Guid.NewGuid():N}.tmp");
        var fileStream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None,
            bufferSize: 81920, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        await using var stream = await content.ReadAsStreamAsync();

        try
        {
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
                    await fileStream.WriteAsync(buffer.AsMemory(0, read));
                    downloaded += read;

                    if (total is > 0)
                    {
                        progress.Report(downloaded / total.Value * 100);
                    }
                }
            } while (isMoreToRead);

            fileStream.Seek(0, SeekOrigin.Begin);
            return fileStream;
        }
        catch
        {
            await fileStream.DisposeAsync();
            throw;
        }
    }
}
