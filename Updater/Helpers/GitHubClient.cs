using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Updater.Models;

namespace Updater.Helpers;

public class GitHubClient : IDisposable
{
    private readonly string _repository;
    private readonly BuildChannel _channel;
    private readonly HttpClient _httpClient = new();
    
    public GitHubClient(string repository, BuildChannel channel, string? username = null, string? token = null)
    {
        _repository = repository;
        _channel = channel;
        _httpClient.BaseAddress = new Uri($"https://api.github.com/repos/{repository}/");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{token}")));
        }
    }
    
    public async Task<RemoteVersionInfo> FetchRemoteVersionAsync()
    {
        AnsiConsole.MarkupLineInterpolated($"[yellow]Checking for updates for {_repository} on the {_channel} channel...[/]");
        
        return await AnsiConsole.Status()
            .StartAsync("[yellow]Fetching version info from remote...[/]", async ctx => 
            {
                // Query the GitHub api to get a list of the latest releases
                var response = await _httpClient.GetAsync("releases");

                // Parse all the releases and versions
                var json = await response.Content.ReadAsStringAsync();
                var releases = JArray.Parse(json)
                    .ToDictionary(r => Version.Parse(r["tag_name"]!.ToString()), r => r);

                // If the channel is staging, get the latest version,
                // while if the channel is release, get the latest stable version
                var latest = _channel == BuildChannel.Staging
                    ? releases.MaxBy(r => r.Key)
                    : releases.Where(r => r.Key.Revision == -1).MaxBy(r => r.Key);
                        
                var remoteVersion = latest.Key;
                var release = latest.Value;
                var build = release["assets"]!.First(t => t["name"]!.ToObject<string>()! == "OpenBullet2.Web.zip");
                var downloadUrl = build["url"]!.ToString();
                var size = build["size"]!.ToObject<double>();
                    
                return new RemoteVersionInfo(remoteVersion, downloadUrl, size);
            });
    }

    public async Task<Stream> DownloadBuildAsync(RemoteVersionInfo remoteVersionInfo)
    {
        return await AnsiConsole.Progress()
            .Columns([
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn()
            ])
            .StartAsync(async ctx => 
            {
                var downloadTask = ctx.AddTask("[green]Downloading[/]");

                var progress = new Progress<double>(p =>
                {
                    downloadTask.Value = p;
                });

                return await DownloadAsync(_httpClient, remoteVersionInfo.DownloadUrl, progress); 
            });
    }

    private static async Task<MemoryStream> DownloadAsync(HttpClient client, string url,
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
    
    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
