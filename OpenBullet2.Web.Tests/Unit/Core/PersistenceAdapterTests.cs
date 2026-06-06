using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenBullet2.Core;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Environment;
using RuriLib.Models.Hits;
using RuriLib.Models.Proxies;

namespace OpenBullet2.Web.Tests.Unit.Core;

public class PersistenceAdapterTests
{
    [Fact]
    public async Task DbProxyRepository_RemoveDuplicatesAsync_RemovesDuplicatesWithinGroup()
    {
        using var database = new TestDatabase();
        using var scope = database.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repo = new DbProxyRepository(context);
        var group = new ProxyGroupEntity { Name = "group" };
        context.ProxyGroups.Add(group);
        context.Proxies.AddRange(
            new ProxyEntity { Group = group, Host = "127.0.0.1", Port = 8080, Type = ProxyType.Http, Username = "u", Password = "p" },
            new ProxyEntity { Group = group, Host = "127.0.0.1", Port = 8080, Type = ProxyType.Http, Username = "u", Password = "p" },
            new ProxyEntity { Group = group, Host = "127.0.0.2", Port = 8080, Type = ProxyType.Http, Username = "u", Password = "p" });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var removed = await repo.RemoveDuplicatesAsync(group.Id);

        Assert.Equal(1, removed);
        Assert.Equal(2, await context.Proxies.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ProxyReloadService_ReloadAsync_FiltersByOwnerAndGroup()
    {
        using var database = new TestDatabase();
        using var scope = database.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var adminGroup = new ProxyGroupEntity { Name = "admin" };
        var guest = new GuestEntity { Username = "guest" };
        var guestGroup = new ProxyGroupEntity { Name = "guest", Owner = guest };
        context.ProxyGroups.AddRange(adminGroup, guestGroup);
        context.Proxies.AddRange(
            new ProxyEntity { Group = adminGroup, Host = "127.0.0.1", Port = 8080, Type = ProxyType.Http },
            new ProxyEntity { Group = guestGroup, Host = "127.0.0.2", Port = 8081, Type = ProxyType.Socks5 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var service = scope.ServiceProvider.GetRequiredService<ProxyReloadService>();

        var adminProxies = (await service.ReloadAsync(-1, 0, TestContext.Current.CancellationToken)).ToList();
        var guestProxies = (await service.ReloadAsync(-1, guest.Id, TestContext.Current.CancellationToken)).ToList();
        var explicitGroupProxies = (await service.ReloadAsync(adminGroup.Id, guest.Id, TestContext.Current.CancellationToken)).ToList();

        Assert.Equal(2, adminProxies.Count);
        Assert.Single(guestProxies);
        Assert.Equal("127.0.0.2", guestProxies[0].Host);
        Assert.Single(explicitGroupProxies);
        Assert.Equal("127.0.0.1", explicitGroupProxies[0].Host);
    }

    [Fact]
    public async Task DatabaseProxyCheckOutput_StoreAsync_UpdatesExistingProxyEntity()
    {
        using var database = new TestDatabase();
        using var scope = database.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var entity = new ProxyEntity
        {
            Host = "127.0.0.1",
            Port = 8080,
            Type = ProxyType.Http,
            Status = ProxyWorkingStatus.Untested
        };
        context.Proxies.Add(entity);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var output = new DatabaseProxyCheckOutput(database.Services.GetRequiredService<IServiceScopeFactory>());
        var checkedAt = DateTime.UtcNow;
        var proxy = new Proxy("127.0.0.1", 8080)
        {
            Id = entity.Id,
            Country = "Italy",
            LastChecked = checkedAt,
            Ping = 123,
            Quality = ProxyQuality.Anonymous,
            WorkingStatus = ProxyWorkingStatus.Working
        };

        await output.StoreAsync(proxy);

        await context.Entry(entity).ReloadAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Italy", entity.Country);
        Assert.Equal(checkedAt, entity.LastChecked);
        Assert.Equal(123, entity.Ping);
        Assert.Equal(ProxyQuality.Anonymous, entity.Quality);
        Assert.Equal(ProxyWorkingStatus.Working, entity.Status);
    }

    [Fact]
    public async Task DatabaseProxyCheckOutput_StoreAsync_MissingProxyDoesNotThrow()
    {
        using var database = new TestDatabase();
        var output = new DatabaseProxyCheckOutput(database.Services.GetRequiredService<IServiceScopeFactory>());
        var proxy = new Proxy("127.0.0.1", 8080) { Id = 12345 };

        var exception = await Record.ExceptionAsync(() => output.StoreAsync(proxy));

        Assert.Null(exception);
    }

    [Fact]
    public async Task DatabaseProxyCheckOutput_StoreAsync_RepositoryFailureDoesNotThrow()
    {
        using var services = new ServiceCollection()
            .AddScoped<IProxyRepository, ThrowingProxyRepository>()
            .BuildServiceProvider();
        var output = new DatabaseProxyCheckOutput(services.GetRequiredService<IServiceScopeFactory>());
        var proxy = new Proxy("127.0.0.1", 8080) { Id = 12345 };

        var exception = await Record.ExceptionAsync(() => output.StoreAsync(proxy));

        Assert.Null(exception);
    }

    [Fact]
    public async Task HitStorageService_StoreAsync_MapsHitToEntity()
    {
        using var database = new TestDatabase();
        var service = database.Services.GetRequiredService<HitStorageService>();
        var hitDate = DateTime.UtcNow;
        var hit = new Hit
        {
            Data = new DataLine("user:pass", new WordlistType()),
            CapturedData = new Dictionary<string, object> { ["token"] = "abc" },
            Proxy = new Proxy("127.0.0.1", 8080),
            Date = hitDate,
            Type = "SUCCESS",
            Config = new Config { Id = "cfg-id", Metadata = new ConfigMetadata { Name = "Config", Category = "Cat" } },
            DataPool = new RangeDataPool(10, 3, pad: true),
            OwnerId = 42
        };

        await service.StoreAsync(hit);

        using var scope = database.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var entity = await context.Hits.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal("user:pass", entity.Data);
        Assert.Equal("token = abc", entity.CapturedData);
        Assert.Equal("127.0.0.1:8080", entity.Proxy);
        Assert.Equal(hitDate, entity.Date);
        Assert.Equal("SUCCESS", entity.Type);
        Assert.Equal("cfg-id", entity.ConfigId);
        Assert.Equal("Config", entity.ConfigName);
        Assert.Equal("Cat", entity.ConfigCategory);
        Assert.Equal(-3, entity.WordlistId);
        Assert.Equal("10|3|1|True", entity.WordlistName);
        Assert.Equal(42, entity.OwnerId);
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection connection = new("DataSource=:memory:");

        public TestDatabase()
        {
            connection.Open();
            Services = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection))
                .AddScoped<IProxyRepository, DbProxyRepository>()
                .AddScoped<IHitRepository, DbHitRepository>()
                .AddScoped<ProxyReloadService>()
                .AddScoped<HitStorageService>()
                .BuildServiceProvider();

            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        }

        public ServiceProvider Services { get; }

        public void Dispose()
        {
            Services.Dispose();
            connection.Dispose();
        }
    }

    private sealed class ThrowingProxyRepository : IProxyRepository
    {
        public Task AddAsync(ProxyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddAsync(IEnumerable<ProxyEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Attach<TEntity>(TEntity entity) where TEntity : Entity
        {
        }

        public Task DeleteAsync(ProxyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(IEnumerable<ProxyEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public IQueryable<ProxyEntity> GetAll() => Enumerable.Empty<ProxyEntity>().AsQueryable();

        public Task<ProxyEntity> GetAsync(int id, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Expected failure");

        public Task<int> RemoveDuplicatesAsync(int groupId) => Task.FromResult(0);

        public Task UpdateAsync(ProxyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(IEnumerable<ProxyEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
