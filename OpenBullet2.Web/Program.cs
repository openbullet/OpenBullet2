using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core;
using OpenBullet2.Core.Helpers;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web;
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
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddControllers()
    .AddJsonOptions(opts => {
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

// builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
    b => b.MigrationsAssembly("OpenBullet2.Core")));

builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// Scoped
builder.Services.AddScoped<IProxyRepository, DbProxyRepository>();
builder.Services.AddScoped<IProxyGroupRepository, DbProxyGroupRepository>();
builder.Services.AddScoped<IHitRepository, DbHitRepository>();
builder.Services.AddScoped<IJobRepository, DbJobRepository>();
builder.Services.AddScoped<IGuestRepository, DbGuestRepository>();
builder.Services.AddScoped<IRecordRepository, DbRecordRepository>();
builder.Services.AddScoped<IWordlistRepository>(service =>
    new HybridWordlistRepository(service.GetService<ApplicationDbContext>(),
    "UserData/Wordlists"));

builder.Services.AddScoped<DataPoolFactoryService>();
builder.Services.AddScoped<ProxySourceFactoryService>();

// Singleton
builder.Services.AddSingleton<IAuthTokenService, AuthTokenService>();
builder.Services.AddSingleton<IAnnouncementService, AnnouncementService>();
builder.Services.AddSingleton<IUpdateService, UpdateService>();
builder.Services.AddSingleton<IConfigRepository>(service =>
    new DiskConfigRepository(service.GetService<RuriLibSettingsService>(),
    "UserData/Configs"));
builder.Services.AddSingleton<ConfigService>();
builder.Services.AddSingleton(service =>
    new ConfigSharingService(service.GetRequiredService<IConfigRepository>(),
    "UserData"));
builder.Services.AddSingleton<ProxyReloadService>();
builder.Services.AddSingleton<JobFactoryService>();
builder.Services.AddSingleton<JobManagerService>();
builder.Services.AddSingleton(service =>
    new JobMonitorService(service.GetService<JobManagerService>(), autoSave: false));
builder.Services.AddSingleton<HitStorageService>();
builder.Services.AddSingleton(_ => new RuriLibSettingsService("UserData"));
builder.Services.AddSingleton(_ => new OpenBulletSettingsService("UserData"));
builder.Services.AddSingleton(_ => new PluginRepository("UserData/Plugins"));
builder.Services.AddSingleton<IRandomUAProvider>(
    _ => new IntoliRandomUAProvider("user-agents.json"));
builder.Services.AddSingleton<IRNGProvider, DefaultRNGProvider>();
builder.Services.AddSingleton<IJobLogger>(service =>
    new FileJobLogger(service.GetService<RuriLibSettingsService>(),
    "UserData/Logs/Jobs"));
builder.Services.AddSingleton<ConfigDebuggerService>();

// Hosted Services
builder.Services.AddHostedService<IUpdateService>(b =>
    b.GetRequiredService<IUpdateService>());

var app = builder.Build();

var versionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in versionDescriptionProvider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }
});

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

var obSettings = app.Services.GetRequiredService<OpenBulletSettingsService>()?.Settings
    ?? throw new Exception($"Missing service: {nameof(OpenBulletSettingsService)}");

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
using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>()?.CreateScope()
    ?? throw new Exception($"Missing service: {nameof(ConfigService)}"))
{
    var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// Load the configs
var configService = app.Services.GetRequiredService<ConfigService>()
    ?? throw new Exception($"Missing service: {nameof(ConfigService)}");

await configService.ReloadConfigs();

// Start the job monitor at the start of the application,
// otherwise it will only be started when navigating to the page
_ = app.Services.GetRequiredService<JobMonitorService>();

Globals.StartTime = DateTime.UtcNow;

app.Run();
