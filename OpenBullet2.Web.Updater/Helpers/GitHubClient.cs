using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using OpenBullet2.Web.Updater.Models;

namespace OpenBullet2.Web.Updater.Helpers;

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

                return await FileDownloader.DownloadAsync(_httpClient, remoteVersionInfo.DownloadUrl, progress); 
            });
    }
    
    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
