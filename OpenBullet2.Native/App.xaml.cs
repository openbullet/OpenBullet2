using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenBullet2.Core;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Logging;
using OpenBullet2.Native.Services;
using RuriLib.Logging;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;

namespace OpenBullet2.Native
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider serviceProvider;
        private readonly IConfiguration config;

        public App()
        {
            Directory.CreateDirectory("UserData");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IConfiguration>(_ => builder.Build());
            ConfigureServices(serviceCollection);
            serviceProvider = serviceCollection.BuildServiceProvider();
            SP.Init(serviceProvider);

            config = SP.GetService<IConfiguration>();
            var workerThreads = config.GetSection("Resources").GetValue("WorkerThreads", 1000);
            var ioThreads = config.GetSection("Resources").GetValue("IOThreads", 1000);
            var connectionLimit = config.GetSection("Resources").GetValue("ConnectionLimit", 1000);

            ThreadPool.SetMinThreads(workerThreads, ioThreads);
            ServicePointManager.DefaultConnectionLimit = connectionLimit;

            // Apply DB migrations or create a DB if it doesn't exist
            using (var serviceScope = serviceProvider.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }

            // Load the configs
            var configService = serviceProvider.GetService<ConfigService>();
            configService.ReloadConfigs().Wait();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Windows
            services.AddSingleton<MainWindow>();

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
            services.AddScoped<IConfigRepository>(_ => new DiskConfigRepository("UserData/Configs"));
            services.AddScoped<IWordlistRepository>(service =>
                new HybridWordlistRepository(service.GetService<ApplicationDbContext>(),
                "UserData/Wordlists"));

            // Singletons
            services.AddSingleton<ViewModelsService>();
            services.AddSingleton<AnnouncementService>();
            services.AddSingleton<UpdateService>();
            services.AddSingleton<ConfigService>();
            services.AddSingleton<ProxyReloadService>();
            services.AddSingleton<JobFactoryService>();
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
                new FileJobLogger(service.GetService<RuriLibSettingsService>(),
                "UserData/Logs/Jobs"));
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = serviceProvider.GetService<MainWindow>();
            mainWindow.Init();
            mainWindow.Show();
        }
    }
}
