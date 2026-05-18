using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Jobs;
using RuriLib.Services;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OpenBullet2.Core.Tests.Services;

public sealed class JobManagerServiceTests
{
    [Fact]
    public async Task SaveMultiRunJobOptionsAsync_WhenJobCompleted_ResetsPersistedSkip()
    {
        using var database = new TestDatabase();
        var manager = new JobManagerService(
            database.Services.GetRequiredService<IServiceScopeFactory>(),
            (JobFactoryService)RuntimeHelpers.GetUninitializedObject(typeof(JobFactoryService)));

        var entity = await database.AddJobEntityAsync(new MultiRunJobOptions
        {
            ConfigId = "cfg",
            DataPool = new RangeDataPoolOptions(),
            Skip = 2
        });

        var settings = CreateSettingsService();
        var job = new MultiRunJob(settings, CreatePluginRepository())
        {
            Id = entity.Id,
            Config = new Config
            {
                Id = "cfg",
                Settings = new ConfigSettings()
            },
            DataPool = new TestDataPool(["one", "two", "three"], settings.Environment.WordlistTypes[0].Name),
            Skip = 2
        };

        typeof(MultiRunJob)
            .GetField("dataTested", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(job, 1);

        await manager.SaveMultiRunJobOptionsAsync(job);

        var saved = await database.GetJobEntityAsync(entity.Id);
        var wrapper = JsonConvert.DeserializeObject<JobOptionsWrapper>(
            saved.JobOptions!, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        var options = Assert.IsType<MultiRunJobOptions>(wrapper!.Options);

        Assert.Equal(0, options.Skip);
    }

    [Fact]
    public async Task SaveMultiRunJobOptionsAsync_PersistsOnlyConfiguredCustomInputsAnswers()
    {
        using var database = new TestDatabase();
        var manager = new JobManagerService(
            database.Services.GetRequiredService<IServiceScopeFactory>(),
            (JobFactoryService)RuntimeHelpers.GetUninitializedObject(typeof(JobFactoryService)));

        var entity = await database.AddJobEntityAsync(new MultiRunJobOptions
        {
            ConfigId = "cfg",
            DataPool = new RangeDataPoolOptions()
        });

        var settings = CreateSettingsService();
        var job = new MultiRunJob(settings, CreatePluginRepository())
        {
            Id = entity.Id,
            Config = new Config
            {
                Id = "cfg",
                Settings = new ConfigSettings
                {
                    InputSettings = new()
                    {
                        CustomInputs =
                        [
                            new()
                            {
                                VariableName = "TEST",
                                Description = "Test input",
                                DefaultAnswer = "default"
                            }
                        ]
                    }
                }
            },
            DataPool = new TestDataPool(["one", "two", "three"], settings.Environment.WordlistTypes[0].Name),
            CustomInputsAnswers = new Dictionary<string, string>
            {
                ["TEST"] = "saved value",
                ["STALE"] = "old value"
            }
        };

        await manager.SaveMultiRunJobOptionsAsync(job);

        var saved = await database.GetJobEntityAsync(entity.Id);
        var wrapper = JsonConvert.DeserializeObject<JobOptionsWrapper>(
            saved.JobOptions!, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        var options = Assert.IsType<MultiRunJobOptions>(wrapper!.Options);

        Assert.Equal("saved value", options.CustomInputsAnswers["TEST"]);
        Assert.False(options.CustomInputsAnswers.ContainsKey("STALE"));
    }

    [Fact]
    public async Task Constructor_WhenPersistedJobOptionsAreMalformed_DoesNotThrow()
    {
        using var database = new TestDatabase();
        await database.AddRawJobEntityAsync("{ this is not valid json }");

        var exception = Record.Exception(() => new JobManagerService(
            database.Services.GetRequiredService<IServiceScopeFactory>(),
            (JobFactoryService)RuntimeHelpers.GetUninitializedObject(typeof(JobFactoryService))));

        Assert.Null(exception);
    }

    [Fact]
    public void RemoveJob_DisposesRemovedJob()
    {
        using var database = new TestDatabase();
        using var manager = new JobManagerService(
            database.Services.GetRequiredService<IServiceScopeFactory>(),
            (JobFactoryService)RuntimeHelpers.GetUninitializedObject(typeof(JobFactoryService)));

        var job = new DisposableTestJob(CreateSettingsService(), CreatePluginRepository());
        manager.AddJob(job);

        manager.RemoveJob(job);

        Assert.True(job.IsDisposed);
    }

    [Fact]
    public void Clear_DisposesTrackedJobs()
    {
        using var database = new TestDatabase();
        using var manager = new JobManagerService(
            database.Services.GetRequiredService<IServiceScopeFactory>(),
            (JobFactoryService)RuntimeHelpers.GetUninitializedObject(typeof(JobFactoryService)));

        var job1 = new DisposableTestJob(CreateSettingsService(), CreatePluginRepository());
        var job2 = new DisposableTestJob(CreateSettingsService(), CreatePluginRepository());
        manager.AddJob(job1);
        manager.AddJob(job2);

        manager.Clear();

        Assert.True(job1.IsDisposed);
        Assert.True(job2.IsDisposed);
        Assert.Empty(manager.Jobs);
    }

    private static RuriLibSettingsService CreateSettingsService()
        => new(Path.Combine(Path.GetTempPath(), $"ob2-jobmanager-settings-{Guid.NewGuid():N}"));

    private static PluginRepository CreatePluginRepository()
        => (PluginRepository)RuntimeHelpers.GetUninitializedObject(typeof(PluginRepository));

    private sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection connection = new("DataSource=:memory:");

        public TestDatabase()
        {
            connection.Open();
            Services = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection))
                .AddScoped<IJobRepository, DbJobRepository>()
                .BuildServiceProvider();

            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        }

        public ServiceProvider Services { get; }

        public async Task<JobEntity> AddJobEntityAsync(MultiRunJobOptions options)
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var entity = new JobEntity
            {
                JobType = JobType.MultiRun,
                JobOptions = JsonConvert.SerializeObject(
                    new JobOptionsWrapper { Options = options },
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })
            };

            context.Jobs.Add(entity);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            return entity;
        }

        public async Task<JobEntity> AddRawJobEntityAsync(string jobOptions)
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var entity = new JobEntity
            {
                JobType = JobType.MultiRun,
                JobOptions = jobOptions
            };

            context.Jobs.Add(entity);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            return entity;
        }

        public async Task<JobEntity> GetJobEntityAsync(int id)
        {
            using var scope = Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            return await repo.GetAsync(id, TestContext.Current.CancellationToken);
        }

        public void Dispose()
        {
            Services.Dispose();
            connection.Dispose();
        }
    }

    private sealed class TestDataPool : DataPool
    {
        public TestDataPool(string[] data, string wordlistType)
        {
            DataList = data;
            Size = data.Length;
            WordlistType = wordlistType;
        }

        public override void Reload()
        {
        }
    }

    private sealed class DisposableTestJob(RuriLibSettingsService settings, PluginRepository pluginRepo)
        : Job(settings, pluginRepo)
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
