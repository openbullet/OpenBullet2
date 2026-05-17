using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenBullet2.Core;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Logging;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.Utils;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using OpenBullet2.Native.Views.Pages;
using OpenBullet2.Native.Views.Pages.Shared;
using RuriLib.Logging;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using OpenBullet2.Core.Models.Proxies;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Settings.Configuration;
using Serilog.Sinks.File;

namespace OpenBullet2.Native;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string LogsDirectoryPath = "UserData/Logs";
    private const string NativeTestModeEnvironmentVariable = "OB2_NATIVE_TEST_MODE";
    private readonly IConfiguration config;
    public static IHost Host { get; private set; } = null!;

    public App()
    {
        Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnTaskException;

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        Directory.CreateDirectory("UserData");
        Directory.CreateDirectory(LogsDirectoryPath);

        config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var workerThreads = config.GetSection("Resources").GetValue("WorkerThreads", 1000);
        var ioThreads = config.GetSection("Resources").GetValue("IOThreads", 1000);

        ThreadPool.SetMinThreads(workerThreads, ioThreads);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(
                config,
                new ConfigurationReaderOptions(
                    typeof(FileLoggerConfigurationExtensions).Assembly,
                    typeof(LoggerEnrichmentConfigurationExtensions).Assembly,
                    typeof(CompactJsonFormatter).Assembly))
            .CreateLogger();

        Log.Debug("Initializing Native host");

        Host = new HostBuilder()
            .UseSerilog(Log.Logger, dispose: true)
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(config);
                ConfigureServices(services);
            })
            .UseDefaultServiceProvider((_, options) =>
            {
                options.ValidateOnBuild = true;
                options.ValidateScopes = true;
            })
            .Build();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Windows and pages
        services.AddSingleton<IUiFactory, UiFactory>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        // EF
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(config.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly("OpenBullet2.Core")));

        // Repositories
        services.AddScoped<IProxyRepository, DbProxyRepository>();
        services.AddScoped<IProxyGroupRepository, DbProxyGroupRepository>();
        services.AddScoped<IHitRepository, DbHitRepository>();
        services.AddScoped<IJobRepository, DbJobRepository>();
        services.AddScoped<IRecordRepository, DbRecordRepository>();
        services.AddSingleton<IConfigRepository>(service =>
            new DiskConfigRepository(service.GetRequiredService<RuriLibSettingsService>(),
            "UserData/Configs"));
        services.AddScoped<IWordlistRepository>(service =>
            new HybridWordlistRepository(service.GetRequiredService<ApplicationDbContext>(),
            "UserData/Wordlists"));

        // Singletons
        services.AddSingleton<VolatileSettingsService>();
        services.AddSingleton<AnnouncementService>();
        services.AddSingleton<UpdateService>();
        services.AddSingleton<ConfigService>();
        services.AddSingleton<ProxyReloadService>();
        services.AddSingleton<ProxyCheckOutputFactory>();
        services.AddSingleton<JobFactoryService>();
        services.AddSingleton<TriggeredActionExecutor>();
        services.AddSingleton<JobManagerService>();
        services.AddSingleton<JobMonitorService>();
        services.AddSingleton<HitStorageService>();
        services.AddSingleton<DataPoolFactoryService>();
        services.AddSingleton<ProxySourceFactoryService>();
        services.AddSingleton(_ => new RuriLibSettingsService("UserData"));
        services.AddSingleton(_ => new OpenBulletSettingsService("UserData"));
        services.AddSingleton(_ => new PluginRepository("UserData/Plugins"));
        services.AddSingleton<IRandomUAProvider>(_ => new IntoliRandomUAProvider("user-agents.json"));
        services.AddSingleton<IRNGProvider, DefaultRNGProvider>();
        services.AddSingleton<MemoryJobLogger>();
        services.AddSingleton<IJobLogger>(service =>
            new FileJobLogger(service.GetRequiredService<RuriLibSettingsService>(),
            "UserData/Logs/Jobs"));

        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<JobsViewModel>();
        services.AddSingleton<ProxiesViewModel>();
        services.AddSingleton<WordlistsViewModel>();
        services.AddSingleton<ConfigsViewModel>();
        services.AddSingleton<HitsViewModel>();
        services.AddSingleton<OBSettingsViewModel>();
        services.AddSingleton<RLSettingsViewModel>();
        services.AddSingleton<PluginsViewModel>();
        services.AddSingleton<ConfigMetadataViewModel>();
        services.AddSingleton<ConfigReadmeViewModel>();
        services.AddSingleton<ConfigStackerViewModel>();
        services.AddSingleton<ConfigSettingsViewModel>();
        services.AddSingleton<DebuggerViewModel>();

        services.AddTransient<ConfigEditorViewModel>();
        services.AddTransient<ConfigLoliCodeViewModel>();
        services.AddTransient<ConfigCSharpCodeViewModel>();
        services.AddTransient<AddBlockDialogViewModel>();
        services.AddTransient<SelectConfigDialogViewModel>();
        services.AddTransient<SelectWordlistDialogViewModel>();
        services.AddTransient<MultiRunJobOptionsViewModel>();
        services.AddTransient<ProxyCheckJobOptionsViewModel>();

        services.AddTransient<Debugger>();
        services.AddTransient<ConfigStacker>();
        services.AddTransient<ConfigLoliCode>();
        services.AddTransient<ConfigCSharpCode>();
        services.AddTransient<ConfigLoliScript>();
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            Log.Information("Starting Native host");
            await Host.StartAsync();
            Log.Information("Native host started");

            if (Environment.GetEnvironmentVariable(NativeTestModeEnvironmentVariable) == "1")
            {
                Log.Information("Native app started in test mode, skipping runtime initialization");
                return;
            }

            using (var serviceScope = Host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Log.Information("Applying database migrations");
                context.Database.Migrate();
                Log.Information("Database migrations completed");
            }

            var configService = Host.Services.GetRequiredService<ConfigService>();
            Log.Information("Reloading configs");
            await configService.ReloadConfigsAsync();
            Log.Information("Reloaded {ConfigCount} configs", configService.Configs.Count);

            AutocompletionProvider.Init(Host.Services.GetRequiredService<OpenBulletSettingsService>());
            Suggestions.Init(
                Host.Services.GetRequiredService<DebuggerViewModel>(),
                Host.Services.GetRequiredService<RuriLibSettingsService>(),
                Host.Services.GetRequiredService<ConfigService>());
            Log.Debug("Initialized Native providers and suggestions");

            _ = Host.Services.GetRequiredService<JobMonitorService>();
            Log.Debug("Job monitor service initialized");

            var mainWindow = Host.Services.GetRequiredService<MainWindow>();
            mainWindow.NavigateTo(MainWindowPage.Home);
            mainWindow.Show();
            Log.Debug("Main window displayed");
        }
        catch (Exception ex)
        {
            ReportCrash(ex);
            Shutdown(-1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("Stopping Native host");
        await Host.StopAsync();
        Log.Information("Native host stopped");
        Host.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ReportCrash(e.Exception);
        e.Handled = true; // Set to false to close the app on exception
    }

    private void OnTaskException(object? sender, UnobservedTaskExceptionEventArgs e) =>
        HandleUnobservedTaskException(e);

    // I decided to disable the code below since usually task exceptions
    // are not critical to the application.

    /*
    if (e.Exception.InnerException is not null)
    {
        if (e.Exception.InnerException is PuppeteerSharp.PuppeteerException // https://github.com/hardkoded/puppeteer-sharp/issues/891
            or System.Net.Sockets.SocketException // Seems like all networking-related things can cause unhandled task exceptions
            or TimeoutException) // This is again thrown by Puppeteer
        {
            return;
        }
    }

    ReportCrash(e.Exception);
    */

    private static void ReportCrash(Exception ex)
    {
        var crashLogPath = Path.Combine(AppContext.BaseDirectory, "crash.log");
        var copyText = ex.ToString();

        Log.Fatal(ex, "Unhandled exception");
        File.WriteAllText(crashLogPath, $"Unhandled exception thrown on {DateTime.Now}{Environment.NewLine}{ex}");

        Alert.Error("Unhandled exception", $"An unhandled exception was thrown, the application will try to continue running." +
            " A crash log was written next to the executable." +
            $" A few details about the exception: {ex.Message}", copyText);
    }

    private static void HandleUnobservedTaskException(UnobservedTaskExceptionEventArgs e)
    {
        Log.Warning(e.Exception, "Observed unhandled task exception");
        e.SetObserved(); // Comment this line to close the app on task exception.
    }
}
