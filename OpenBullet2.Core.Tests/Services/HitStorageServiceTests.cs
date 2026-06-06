using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Environment;
using RuriLib.Models.Hits;
using RuriLib.Models.Proxies;

namespace OpenBullet2.Core.Tests.Services;

public sealed class HitStorageServiceTests
{
    [Fact]
    public async Task StoreAsync_WhenCalledConcurrently_AllowsConcurrentRepositoryWrites()
    {
        var tracker = new TrackingRepositoryState();
        using var services = new ServiceCollection()
            .AddSingleton(tracker)
            .AddScoped<IHitRepository, TrackingHitRepository>()
            .BuildServiceProvider();

        var service = new HitStorageService(services.GetRequiredService<IServiceScopeFactory>());
        var storeTasks = Enumerable.Range(1, 4)
            .Select(i => service.StoreAsync(CreateHit(i)))
            .ToArray();

        await tracker.AllWritesReached.Task.WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(4, tracker.TotalWritesStarted);
        Assert.True(tracker.MaxConcurrentWrites > 1);

        tracker.ReleaseWrites.TrySetResult();
        await Task.WhenAll(storeTasks).WaitAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task StoreAsync_WhenCalledConcurrently_PersistsAllHits()
    {
        using var database = new TestDatabase();
        var service = database.Services.GetRequiredService<HitStorageService>();
        var storeTasks = Enumerable.Range(1, 10)
            .Select(i => service.StoreAsync(CreateHit(i)))
            .ToArray();

        await Task.WhenAll(storeTasks).WaitAsync(TestContext.Current.CancellationToken);

        using var scope = database.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var entities = await context.Hits
            .OrderBy(h => h.OwnerId)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(10, entities.Count);
        Assert.Equal("user1:pass1", entities.First().Data);
        Assert.Equal("user10:pass10", entities.Last().Data);
    }

    [Fact]
    public async Task StoreAsync_RangeDataPoolsWithDifferentSteps_PersistDifferentWordlistNames()
    {
        using var database = new TestDatabase();
        var service = database.Services.GetRequiredService<HitStorageService>();

        await service.StoreAsync(CreateHit(1, new RangeDataPool(10, 3, 1, true)))
            .WaitAsync(TestContext.Current.CancellationToken);
        await service.StoreAsync(CreateHit(2, new RangeDataPool(10, 3, 2, true)))
            .WaitAsync(TestContext.Current.CancellationToken);

        using var scope = database.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var wordlistNames = await context.Hits
            .OrderBy(h => h.OwnerId)
            .Select(h => h.WordlistName)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["10|3|1|True", "10|3|2|True"], wordlistNames);
    }

    [Fact]
    public async Task StoreAsync_WhenUsingFileBackedSqliteAndCalledConcurrently_PersistsAllHits()
    {
        using var database = new TestDatabase(useFileBackedSqlite: true);
        var service = database.Services.GetRequiredService<HitStorageService>();
        var storeTasks = Enumerable.Range(1, 100)
            .Select(i => service.StoreAsync(CreateHit(i)))
            .ToArray();

        await Task.WhenAll(storeTasks).WaitAsync(TestContext.Current.CancellationToken);

        using var scope = database.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hitCount = await context.Hits.CountAsync(TestContext.Current.CancellationToken);

        Assert.Equal(100, hitCount);
    }

    private static Hit CreateHit(int index, RangeDataPool? dataPool = null) => new()
    {
        Data = new DataLine($"user{index}:pass{index}", new WordlistType()),
        CapturedData = new Dictionary<string, object> { ["token"] = $"abc{index}" },
        Proxy = new Proxy("127.0.0.1", 8080 + index),
        Date = DateTime.UtcNow,
        Type = "SUCCESS",
        Config = new Config
        {
            Id = $"cfg-{index}",
            Metadata = new ConfigMetadata { Name = "Config", Category = "Cat" }
        },
        DataPool = dataPool ?? new RangeDataPool(index, 3, pad: true),
        OwnerId = index
    };

    private sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection? connection;
        private readonly string? databasePath;

        public TestDatabase(bool useFileBackedSqlite = false)
        {
            if (useFileBackedSqlite)
            {
                databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
            }
            else
            {
                connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();
            }

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            {
                if (useFileBackedSqlite)
                {
                    options.UseSqlite($"Data Source={databasePath}");
                }
                else
                {
                    options.UseSqlite(connection!);
                }
            });

            Services = serviceCollection
                .AddScoped<IHitRepository, DbHitRepository>()
                .AddSingleton<HitStorageService>()
                .BuildServiceProvider();

            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        }

        public ServiceProvider Services { get; }

        public void Dispose()
        {
            Services.Dispose();
            connection?.Dispose();

            if (databasePath is not null)
            {
                TryDeleteFile(databasePath);
            }
        }

        private static void TryDeleteFile(string path)
        {
            for (var i = 0; i < 10; i++)
            {
                if (!File.Exists(path))
                {
                    return;
                }

                try
                {
                    File.Delete(path);
                    return;
                }
                catch (IOException) when (i < 9)
                {
                    Thread.Sleep(50);
                }
                catch (IOException)
                {
                    return;
                }
            }
        }
    }

    private sealed class TrackingRepositoryState
    {
        private int currentWrites;
        private int maxConcurrentWrites;

        public TaskCompletionSource AllWritesReached { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseWrites { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int TotalWritesStarted => totalWritesStarted;
        public int MaxConcurrentWrites => maxConcurrentWrites;

        private int totalWritesStarted;

        public void EnterWrite()
        {
            Interlocked.Increment(ref totalWritesStarted);
            var current = Interlocked.Increment(ref currentWrites);

            while (true)
            {
                var observedMax = maxConcurrentWrites;
                if (current <= observedMax)
                {
                    break;
                }

                if (Interlocked.CompareExchange(ref maxConcurrentWrites, current, observedMax) == observedMax)
                {
                    break;
                }
            }

            if (TotalWritesStarted == 4)
            {
                AllWritesReached.TrySetResult();
            }
        }

        public void ExitWrite()
            => Interlocked.Decrement(ref currentWrites);
    }

    private sealed class TrackingHitRepository(TrackingRepositoryState state) : IHitRepository
    {
        public async Task AddAsync(HitEntity entity, CancellationToken cancellationToken = default)
        {
            state.EnterWrite();

            try
            {
                await state.ReleaseWrites.Task.WaitAsync(cancellationToken);
            }
            finally
            {
                state.ExitWrite();
            }
        }

        public Task AddAsync(IEnumerable<HitEntity> entities, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public void Attach<TEntity>(TEntity entity) where TEntity : Entity
        {
        }

        public Task<long> CountAsync() => Task.FromResult(0L);

        public Task DeleteAsync(HitEntity entity, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task DeleteAsync(IEnumerable<HitEntity> entities, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IQueryable<HitEntity> GetAll() => Enumerable.Empty<HitEntity>().AsQueryable();

        public Task<HitEntity> GetAsync(int id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task PurgeAsync() => throw new NotSupportedException();

        public Task UpdateAsync(HitEntity entity, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UpdateAsync(IEnumerable<HitEntity> entities, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
