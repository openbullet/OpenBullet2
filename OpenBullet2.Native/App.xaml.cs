using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OpenBullet2.Core;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Logging;
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
        private readonly ServiceProvider _serviceProvider;
        private readonly JObject _config;

        public App()
        {
            Directory.CreateDirectory("UserData");

            _config = JObject.Parse(File.ReadAllText("appsettings.json"));

            ThreadPool.SetMinThreads(_config["Resources"].Value<int>("WorkerThreads"), _config["Resources"].Value<int>("IOThreads"));
            ServicePointManager.DefaultConnectionLimit = _config["Resources"].Value<int>("ConnectionLimit");

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // Apply DB migrations or create a DB if it doesn't exist
            using (var serviceScope = _serviceProvider.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            }

            // Load the configs
            var configService = _serviceProvider.GetService<ConfigService>();
            configService.ReloadConfigs().Wait();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Windows
            services.AddSingleton<MainWindow>();

            // EF
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_config["ConnectionStrings"].Value<string>("DefaultConnection")));

            // Repositories
            services.AddScoped<IProxyRepository, DbProxyRepository>();
            services.AddScoped<IProxyGroupRepository, DbProxyGroupRepository>();
            services.AddScoped<IHitRepository, DbHitRepository>();
            services.AddScoped<IJobRepository, DbJobRepository>();
            services.AddScoped<IGuestRepository, DbGuestRepository>();
            services.AddScoped<IRecordRepository, DbRecordRepository>();
            services.AddScoped<IConfigRepository>(_ => new DiskConfigRepository("UserData/Configs"));
            services.AddScoped<IWordlistRepository>(service =>
                new HybridWordlistRepository(service.GetService<ApplicationDbContext>(),
                "UserData/Wordlists"));

            // Singletons
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
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}
