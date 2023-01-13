using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core;
using OpenBullet2.Core.Helpers;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Utils;
using RuriLib.Helpers;
using RuriLib.Logging;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Singleton (they CANNOT consume scoped classes!)
// Workaround: https://stackoverflow.com/questions/51572637/access-dbcontext-service-from-background-task
// https://stackoverflow.com/questions/72113872/cannot-consume-scoped-service-applicationdbcontext-from-singleton-microsoft-e
// (we should use IHostedService btw, change OpenBullet2.Core code!)
// (basically use the scope and get the service from within the singleton's ctor)
builder.Services.AddSingleton<IConfigRepository>(service =>
    new DiskConfigRepository(service.GetService<RuriLibSettingsService>(),
    "UserData/Configs"));
builder.Services.AddSingleton<ConfigService>();
builder.Services.AddSingleton<ProxyReloadService>();
builder.Services.AddSingleton<JobFactoryService>();
builder.Services.AddSingleton<JobManagerService>();
builder.Services.AddSingleton<JobMonitorService>();
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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.UseRouting();

var obSettings = app.Services.GetService<OpenBulletSettingsService>()?.Settings
    ?? throw new Exception($"Missing service: {nameof(OpenBulletSettingsService)}");

if (RootChecker.IsRoot())
{
    Console.WriteLine(RootUtils.RootWarning);
}

if (obSettings.SecuritySettings.HttpsRedirect)
{
    app.UseHttpsRedirection();
}

// Apply DB migrations or create a DB if it doesn't exist
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope()
    ?? throw new Exception($"Missing service: {nameof(ConfigService)}"))
{
    var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// Load the configs
var configService = app.Services.GetService<ConfigService>()
    ?? throw new Exception($"Missing service: {nameof(ConfigService)}");

await configService.ReloadConfigs();

// Start the job monitor at the start of the application,
// otherwise it will only be started when navigating to the page
_ = app.Services.GetService<JobMonitorService>();

app.Run();
