using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Info;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;
using PuppeteerExtraSharp;
using System.Net;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace OpenBullet2.Web.Controllers;

[Guest]
public class InfoController : ApiController
{
    private readonly IAnnouncementService _announcementService;
    private readonly IUpdateService _updateService;
    private readonly IMapper _mapper;

    public InfoController(IAnnouncementService announcementService,
        IUpdateService updateService, IMapper mapper)
    {
        _announcementService = announcementService;
        _updateService = updateService;
        _mapper = mapper;
    }

    [HttpGet("server")]
    public ActionResult<ServerInfoDto> GetServerInfo()
    {
        var version = System.Reflection.Assembly
            .GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 1);

        var buildDate = new DateTime(2000, 1, 1)
            .AddDays(version.Build).AddSeconds(version.Revision * 2);
        
        var buildNumber = $"{version.Build}.{version.Revision}";

        var ip = HttpContext.Connection?.RemoteIpAddress
            ?? IPAddress.Parse("127.0.0.1");

        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }

        return new ServerInfoDto
        {
            LocalUtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now),
            StartTime = Globals.StartTime,
            OperatingSystem = GetOperatingSystem(),
            CurrentWorkingDirectory = Directory.GetCurrentDirectory(),
            BuildNumber = buildNumber,
            BuildDate = buildDate,
            ClientIpAddress = ip.ToString()
        };
    }

    [HttpGet("announcement")]
    public async Task<ActionResult<AnnouncementDto>> GetAnnouncement()
    {
        var markdown = await _announcementService.FetchAnnouncement();

        return new AnnouncementDto
        {
            MarkdownText = markdown,
            LastFetched = _announcementService.LastFetched
        };
    }

    [HttpGet("changelog")]
    public async Task<ActionResult<ChangelogDto>> GetChangelog(string? version)
    {
        var markdown = string.Empty;

        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");

        // If a version was not provided, use the current version
        version ??= _updateService.CurrentVersion.ToString();

        try
        {
            var url = $"https://raw.githubusercontent.com/openbullet/OpenBullet2/master/Changelog/{version}.md";
            using var response = await client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new RemoteResourceNotFound(
                    ErrorCode.REMOTE_RESOURCE_NOT_FOUND,
                    $"Changelog for version {version}", url);
            }

            markdown = await response.Content.ReadAsStringAsync();
        }
        catch (RemoteResourceNotFound)
        {
            // Rethrow this since it is handled by the middleware
            throw;
        }
        catch
        {
            markdown = "Could not retrieve the changelog";
        }

        return new ChangelogDto 
        {
            MarkdownText = markdown,
            Version = version
        };
    }

    [HttpGet("update")]
    public ActionResult<UpdateInfoDto> GetUpdateInfo() => new UpdateInfoDto
    {
        CurrentVersion = _updateService.CurrentVersion.ToString(),
        RemoteVersion = _updateService.RemoteVersion.ToString(),
        IsUpdateAvailable = _updateService.IsUpdateAvailable,
        CurrentVersionType = _updateService.CurrentVersionType
    };
    

    private static string GetOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Environment.OSVersion.VersionString;

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return $"macOS {Environment.OSVersion.Version}";

        else
            return RuntimeInformation.OSDescription;
    }
}
