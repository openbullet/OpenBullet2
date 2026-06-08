using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Info;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;
using RuriLib.Services;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenBullet2.Web.Auth;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Get info about the server.
/// </summary>
/// <remarks></remarks>
[TypeFilter<GuestFilter>]
[ApiVersion("1.0")]
public class InfoController(IAnnouncementService announcementService,
    IChangelogService changelogService, IUpdateService updateService,
    JobManagerService jobManager, IProxyRepository proxyRepo,
    IWordlistRepository wordlistRepo, IHitRepository hitRepo,
    IGuestRepository guestRepo, ConfigService configService,
    PluginRepository pluginRepository) : ApiController
{
    private readonly IAnnouncementService _announcementService = announcementService;
    private readonly IChangelogService _changelogService = changelogService;
    private readonly ConfigService _configService = configService;
    private readonly IGuestRepository _guestRepo = guestRepo;
    private readonly IHitRepository _hitRepo = hitRepo;
    private readonly JobManagerService _jobManager = jobManager;
    private readonly PluginRepository _pluginRepository = pluginRepository;
    private readonly IProxyRepository _proxyRepo = proxyRepo;
    private readonly IUpdateService _updateService = updateService;
    private readonly IWordlistRepository _wordlistRepo = wordlistRepo;

    /// <summary>
    /// Get information about the server and the environment.
    /// </summary>
    [HttpGet("server")]
    [MapToApiVersion("1.0")]
    public ActionResult<ServerInfoDto> GetServerInfo()
    {
        var version = Assembly
            .GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 1);

        var buildDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
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

    /// <summary>
    /// Get the current announcement.
    /// </summary>
    [HttpGet("announcement")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AnnouncementDto>> GetAnnouncement()
    {
        var markdown = await _announcementService.FetchAnnouncementAsync();

        return new AnnouncementDto { MarkdownText = markdown, LastFetched = _announcementService.LastFetched };
    }

    /// <summary>
    /// Get the complete changelog bundled with the software.
    /// </summary>
    [HttpGet("changelog")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ChangelogDto>> GetChangelog(CancellationToken cancellationToken)
    {
        string markdown;

        try
        {
            markdown = await _changelogService.FetchChangelogAsync(cancellationToken);
        }
        catch
        {
            markdown = "Could not retrieve the changelog";
        }

        return new ChangelogDto { MarkdownText = markdown };
    }

    /// <summary>
    /// Get information about the availability of a new update.
    /// </summary>
    [HttpGet("update")]
    [MapToApiVersion("1.0")]
    public ActionResult<UpdateInfoDto> GetUpdateInfo() => new UpdateInfoDto
    {
        CurrentVersion = _updateService.CurrentVersion.ToString(),
        RemoteVersion = _updateService.RemoteVersion.ToString(),
        IsUpdateAvailable = _updateService.IsUpdateAvailable
    };

    /// <summary>
    /// Get information about your collection of wordlists, proxies etc.
    /// </summary>
    [HttpGet("collection")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<CollectionInfoDto>> GetCollectionInfo(CancellationToken cancellationToken)
    {
        var apiUser = HttpContext.GetApiUser();

        var jobCount = apiUser.Role is UserRole.Admin
            ? _jobManager.Jobs.Count()
            : _jobManager.Jobs.Count(j => j.OwnerId == apiUser.Id);

        var proxyCount = apiUser.Role is UserRole.Admin
            ? await _proxyRepo.GetAll().CountAsync(cancellationToken)
            : await _proxyRepo.GetAll().CountAsync(p => p.Group != null
                    && p.Group.Owner != null
                    && p.Group.Owner.Id == apiUser.Id, cancellationToken);

        var wordlistCount = apiUser.Role is UserRole.Admin
            ? await _wordlistRepo.GetAll().CountAsync(cancellationToken)
            : await _wordlistRepo.GetAll()
                .CountAsync(w => w.Owner != null && w.Owner.Id == apiUser.Id, cancellationToken);

        var wordlistLines = apiUser.Role is UserRole.Admin
            ? await _wordlistRepo.GetAll().SumAsync(w => w.Total, cancellationToken)
            : await _wordlistRepo.GetAll()
                .Where(w => w.Owner != null && w.Owner.Id == apiUser.Id)
                .SumAsync(w => w.Total, cancellationToken);

        var hitCount = apiUser.Role is UserRole.Admin
            ? await _hitRepo.GetAll().CountAsync(cancellationToken)
            : await _hitRepo.GetAll().CountAsync(h => h.OwnerId == apiUser.Id, cancellationToken);

        var configCount = _configService.Configs.Count;

        var guestCount = apiUser.Role is UserRole.Admin
            ? await _guestRepo.GetAll().CountAsync(cancellationToken)
            : 1; // The guest shouldn't see the total number of guests

        var pluginCount = apiUser.Role is UserRole.Admin
            ? _pluginRepository.GetPlugins().Count()
            : 0; // The guest shouldn't see the total number of plugins

        return new CollectionInfoDto
        {
            JobsCount = jobCount,
            ProxiesCount = proxyCount,
            WordlistsCount = wordlistCount,
            WordlistsLines = wordlistLines,
            HitsCount = hitCount,
            ConfigsCount = configCount,
            GuestsCount = guestCount,
            PluginsCount = pluginCount
        };
    }

    private static string GetOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.OSVersion.VersionString;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"macOS {Environment.OSVersion.Version}";
        }

        return RuntimeInformation.OSDescription;
    }
}
