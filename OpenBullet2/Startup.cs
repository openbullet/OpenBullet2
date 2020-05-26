using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor.FileReader;
using Blazored.Modal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using OpenBullet2.Models.Jobs;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Services;

namespace OpenBullet2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });
            services.AddBlazoredModal();
            services.AddFileReaderService();
            services.AddScoped<IConfigRepository, DiskConfigRepository>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            services.AddScoped<IProxyRepository, DbProxyRepository>();
            services.AddScoped<IProxyGroupRepository, DbProxyGroupRepository>();
            services.AddScoped<IWordlistRepository, HybridWordlistRepository>();
            services.AddScoped<IHitRepository, DbHitRepository>();
            services.AddScoped<IJobRepository, DbJobRepository>();

            // Singletons
            services.AddSingleton<MetricsService>();
            services.AddSingleton<RuriLibSettingsService>();
            services.AddSingleton<PersistentSettingsService>();
            services.AddSingleton<VolatileSettingsService>();
            services.AddSingleton<ConfigService>();
            services.AddSingleton<JobManagerService>();
            services.AddSingleton<SingletonDbHitRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}");

                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            // Load the configs
            var configService = app.ApplicationServices.GetService<ConfigService>();
            var configRepo = app.ApplicationServices.GetService<IConfigRepository>();
            configService.Configs = configRepo.GetAll().Result;

            // Load the settings
            var settingsService = app.ApplicationServices.GetService<RuriLibSettingsService>();

            // Load the singleton hits repo (the one background jobs use to store hits)
            var hitRepo = app.ApplicationServices.GetService<SingletonDbHitRepository>();

            // Restore the saved jobs
            var jobManager = app.ApplicationServices.GetService<JobManagerService>();
            var jobRepo = app.ApplicationServices.GetService<IJobRepository>();
            RestoreJobs(jobRepo, jobManager, configService, settingsService, hitRepo);
        }

        private void RestoreJobs(IJobRepository jobRepo, JobManagerService jobManager, ConfigService configService,
            RuriLibSettingsService settingsService, SingletonDbHitRepository hitRepo)
        {
            var entries = jobRepo.GetAll().ToList();
            var factory = new JobFactory(configService, settingsService, hitRepo);
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            foreach (var entry in entries)
            {
                var options = JsonConvert.DeserializeObject<JobOptionsWrapper>(entry.JobOptions, settings).Options;
                var job = factory.FromOptions(entry.Id, options);
                jobManager.Jobs.Add(job);
            }
        }
    }
}
