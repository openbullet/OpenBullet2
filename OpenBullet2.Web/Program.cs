using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core;
using OpenBullet2.Core.Helpers;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web;
using OpenBullet2.Web.Controllers;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Middleware;
using OpenBullet2.Web.Services;
using OpenBullet2.Web.SignalR;
using OpenBullet2.Web.Utils;
using RuriLib.Helpers;
using RuriLib.Logging;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.OpenApi.Models;
using OpenBullet2.Core.Models.Proxies;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Globals.UserDataFolder = builder.Configuration.GetSection("Settings")
    .GetValue<string>("UserDataFolder") ?? "UserData";

// Configuration tweaks
var workerThreads = builder.Configuration.GetSection("Resources").GetValue("WorkerThreads", 1000);
var ioThreads = builder.Configuration.GetSection("Resources").GetValue("IOThreads", 1000);
var connectionLimit = builder.Configuration.GetSection("Resources").GetValue("ConnectionLimit", 1000);

ThreadPool.SetMinThreads(workerThreads, ioThreads);
ServicePointManager.DefaultConnectionLimit = connectionLimit;

builder.Services.Configure<FormOptions>(x =>
{
    x.MultipartBodyLengthLimit = long.MaxValue;
});

// Add services to the container.

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        var enumConverter = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);
        opts.JsonSerializerOptions.Converters.Add(enumConverter);
    });

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        var enumConverter = new JsonStringEnumConverter(JsonNamingPolicy.CamelCase);
        options.PayloadSerializerOptions.Converters.Add(enumConverter);
    });

// Swagger with versioning implemented according to this guide
// https://referbruv.com/blog/integrating-aspnet-core-api-versions-with-swagger-ui/
builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Api Key", new OpenApiSecurityScheme
    {
        Description = "Enter the API key you configured in OB Settings > Security > Admin API Key",
        Name = "X-Api-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Api Key"
                },
                Scheme = "ApiKeyScheme",
                Name = "X-Api-Key",
                In = ParameterLocation.Header,
                
            },
            new List<string>()
        }
    });
});
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
        b => b.MigrationsAssembly("OpenBullet2.Core")));

builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

// Scoped
builder.Services.AddScoped<IProxyRepository, DbProxyRepository>();
builder.Services.AddScoped<IProxyGroupRepository, DbProxyGroupRepository>();
builder.Services.AddScoped<IHitRepository, DbHitRepository>();
builder.Services.AddScoped<IJobRepository, DbJobRepository>();
builder.Services.AddScoped<IGuestRepository, DbGuestRepository>();
builder.Services.AddScoped<IRecordRepository, DbRecordRepository>();
builder.Services.AddScoped<IWordlistRepository>(service =>
    new HybridWordlistRepository(service.GetService<ApplicationDbContext>(),
        $"{Globals.UserDataFolder}/Wordlists"));

builder.Services.AddScoped<DataPoolFactoryService>();
builder.Services.AddScoped<ProxySourceFactoryService>();

// Singleton
builder.Services.AddSingleton(sp => sp); // The service provider itself
builder.Services.AddSingleton<IAuthTokenService, AuthTokenService>();
builder.Services.AddSingleton<IAnnouncementService, AnnouncementService>();
builder.Services.AddSingleton<IUpdateService, UpdateService>();
builder.Services.AddSingleton<PerformanceMonitorService>();
builder.Services.AddSingleton<IConfigRepository>(service =>
    new DiskConfigRepository(service.GetService<RuriLibSettingsService>(),
        $"{Globals.UserDataFolder}/Configs"));
builder.Services.AddSingleton<ConfigService>();
builder.Services.AddSingleton(service =>
    new ConfigSharingService(service.GetRequiredService<IConfigRepository>(),
        service.GetRequiredService<ILogger<ConfigSharingService>>(),
        Globals.UserDataFolder));
builder.Services.AddSingleton<ProxyReloadService>();
builder.Services.AddSingleton<JobFactoryService>();
builder.Services.AddSingleton<ProxyCheckOutputFactory>();
builder.Services.AddSingleton<JobManagerService>();
builder.Services.AddSingleton(service =>
    new JobMonitorService(service.GetService<JobManagerService>(),
        $"{Globals.UserDataFolder}/triggeredActions.json", false));
builder.Services.AddSingleton<HitStorageService>();
builder.Services.AddSingleton(_ => new RuriLibSettingsService(Globals.UserDataFolder));
builder.Services.AddSingleton(_ => new OpenBulletSettingsService(Globals.UserDataFolder));
builder.Services.AddSingleton(_ => new PluginRepository($"{Globals.UserDataFolder}/Plugins"));
builder.Services.AddSingleton(_ => new ThemeService($"{Globals.UserDataFolder}/Themes"));
builder.Services.AddSingleton<IRandomUAProvider>(
    _ => new IntoliRandomUAProvider("user-agents.json"));
builder.Services.AddSingleton<IRNGProvider, DefaultRNGProvider>();
builder.Services.AddSingleton<IJobLogger>(service =>
    new FileJobLogger(service.GetService<RuriLibSettingsService>(),
        $"{Globals.UserDataFolder}/Logs/Jobs"));
builder.Services.AddSingleton<ConfigDebuggerService>();
builder.Services.AddSingleton<ProxyCheckJobService>();
builder.Services.AddSingleton<MultiRunJobService>();
builder.Services.AddSingleton<LoliCodeAutocompletionService>();

// HttpClient
builder.Services.AddHttpClient<InfoController>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(Globals.UserAgent);
});
builder.Services.AddHttpClient<ProxyController>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(Globals.UserAgent);
});
builder.Services.AddHttpClient<ConfigController>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(Globals.UserAgent);
});

// Hosted Services
builder.Services.AddHostedService(
    b => b.GetRequiredService<IUpdateService>());
builder.Services.AddHostedService(
    b => b.GetRequiredService<PerformanceMonitorService>());

var app = builder.Build();

var versionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var groupName in versionDescriptionProvider.ApiVersionDescriptions
                 .Select(description => description.GroupName))
    {
        options.SwaggerEndpoint(
            $"/swagger/{groupName}/swagger.json",
            groupName.ToUpperInvariant());
    }
});

var allowedOrigin = app.Configuration.GetSection("Settings")
    .GetValue<string>("AllowedOrigin") ?? "http://localhost:4200";

app.UseCors(o => o
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials() // Needed for SignalR (it uses sticky cookie-based sessions for reconnection)
    .WithOrigins(allowedOrigin)
    .WithExposedHeaders("Content-Disposition", "X-Application-Warning", "X-New-Jwt")
);

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<AuthTokenVerificationMiddleware>();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ConfigDebuggerHub>("hubs/config-debugger", options =>
{
    // Incoming messages <= 1 MB
    options.ApplicationMaxBufferSize = 1_000_000;

    // Outgoing messages <= 10 MB
    options.TransportMaxBufferSize = 10_000_000;
});

app.MapHub<ProxyCheckJobHub>("hubs/proxy-check-job", options =>
{
    // Incoming messages <= 1 MB
    options.ApplicationMaxBufferSize = 1_000_000;

    // Outgoing messages <= 10 MB
    options.TransportMaxBufferSize = 10_000_000;
});

app.MapHub<MultiRunJobHub>("hubs/multi-run-job", options =>
{
    // Incoming messages <= 1 MB
    options.ApplicationMaxBufferSize = 1_000_000;

    // Outgoing messages <= 10 MB
    options.TransportMaxBufferSize = 10_000_000;
});

app.MapHub<SystemPerformanceHub>("hubs/system-performance");

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToController(
    nameof(FallbackController.Index),
    nameof(FallbackController).Replace("Controller", "")
);

var obSettings = app.Services.GetRequiredService<OpenBulletSettingsService>().Settings;
var updateService = app.Services.GetRequiredService<IUpdateService>();

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine($"""
                     ____                   ____        ____     __     ___ 
                    / __ \____  ___  ____  / __ )__  __/ / /__  / /_   |__ \
                   / / / / __ \/ _ \/ __ \/ __  / / / / / / _ \/ __/   __/ /
                  / /_/ / /_/ /  __/ / / / /_/ / /_/ / / /  __/ /_    / __/ 
                  \____/ .___/\___/_/ /_/_____/\__,_/_/_/\___/\__/   /____/ 
                      /_/                                                                               
                      
                  v{updateService.CurrentVersion} [{updateService.CurrentVersionType}]    
                  """);
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("""
                  
                  ----------
                  DISCLAIMER
                  ----------
                  Performing attacks on sites you do not own (or you do not have permission to test) is illegal!
                  The developer will not be held responsible for improper use of this software.
                  
                  """);

Console.ForegroundColor = ConsoleColor.White;
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    Console.WriteLine($"DO NOT CLOSE THIS WINDOW{Environment.NewLine}");
}

if (RootChecker.IsRoot())
{
    Console.WriteLine(RootUtils.RootWarning);
}

if (obSettings.SecuritySettings.HttpsRedirect)
{
    app.UseHttpsRedirection();
}

// Cache the polymorphic types
PolyDtoCache.Scan();

// Apply DB migrations or create a DB if it doesn't exist
using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}

// Load the configs
var configService = app.Services.GetRequiredService<ConfigService>();
await configService.ReloadConfigsAsync();

// Register the block snippets
var autocompletionProvider = app.Services.GetRequiredService<LoliCodeAutocompletionService>();
autocompletionProvider.Init();

// Start the job monitor at the start of the application,
// otherwise it will only be started when navigating to the page
_ = app.Services.GetRequiredService<JobMonitorService>();

Globals.StartTime = DateTime.UtcNow;

await app.RunAsync();

// This makes Program visible for integration tests
#pragma warning disable S1118
/// <summary>
/// The main entry point for the application.
/// </summary>
public partial class Program
{
}
#pragma warning restore S1118
