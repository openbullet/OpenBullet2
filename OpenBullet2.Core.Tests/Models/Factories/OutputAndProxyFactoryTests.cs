using Microsoft.Extensions.DependencyInjection;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Models.Proxies.Sources;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using RuriLib.Models.Hits.HitOutputs;
using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;

namespace OpenBullet2.Core.Tests.Models.Factories;

public class OutputAndProxyFactoryTests
{
    [Fact]
    public void ProxyFactory_FromEntity_MapsRuntimeProxy()
    {
        var checkedAt = DateTime.UtcNow;
        var entity = new ProxyEntity
        {
            Id = 99,
            Host = "127.0.0.1",
            Port = 8080,
            Type = ProxyType.Socks5,
            Username = "user",
            Password = "pass",
            Country = null,
            Status = ProxyWorkingStatus.Working,
            LastChecked = checkedAt,
            Ping = 42
        };

        var proxy = ProxyFactory.FromEntity(entity);

        Assert.Equal(99, proxy.Id);
        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(8080, proxy.Port);
        Assert.Equal(ProxyType.Socks5, proxy.Type);
        Assert.Equal("user", proxy.Username);
        Assert.Equal("pass", proxy.Password);
        Assert.Equal("Unknown", proxy.Country);
        Assert.Equal(ProxyWorkingStatus.Working, proxy.WorkingStatus);
        Assert.Equal(checkedAt, proxy.LastChecked);
        Assert.Equal(42, proxy.Ping);
    }

    [Fact]
    public void HitOutputFactory_FromOptions_ReturnsExpectedOutputTypes()
    {
        var factory = new HitOutputFactory(null!);

        Assert.IsType<DatabaseHitOutput>(factory.FromOptions(new DatabaseHitOutputOptions()));
        Assert.IsType<FileSystemHitOutput>(factory.FromOptions(new FileSystemHitOutputOptions { BaseDir = "hits" }));
        Assert.IsType<DiscordWebhookHitOutput>(factory.FromOptions(new DiscordWebhookHitOutputOptions
        {
            Webhook = "https://example.com/webhook",
            Username = "bot",
            AvatarUrl = "https://example.com/avatar.png"
        }));
        Assert.IsType<TelegramBotHitOutput>(factory.FromOptions(new TelegramBotHitOutputOptions
        {
            Token = "token",
            ChatId = 123
        }));
        Assert.IsType<CustomWebhookHitOutput>(factory.FromOptions(new CustomWebhookHitOutputOptions
        {
            Url = "https://example.com/hit",
            User = "user"
        }));
    }

    [Fact]
    public void ProxyCheckOutputFactory_FromOptions_ReturnsDatabaseOutput()
    {
        using var services = new ServiceCollection()
            .AddScoped<IProxyRepository, StubProxyRepository>()
            .BuildServiceProvider();
        var factory = new ProxyCheckOutputFactory(services.GetRequiredService<IServiceScopeFactory>());

        var output = factory.FromOptions(new DatabaseProxyCheckOutputOptions());

        Assert.IsType<DatabaseProxyCheckOutput>(output);
    }

    [Fact]
    public async Task ProxySourceFactoryService_FromOptions_ReturnsExpectedSources()
    {
        var factory = new ProxySourceFactoryService(null!);

        var remote = await factory.FromOptions(new RemoteProxySourceOptions
        {
            Url = "https://example.com/proxies.txt",
            DefaultType = ProxyType.Socks5
        });
        var file = await factory.FromOptions(new FileProxySourceOptions
        {
            FileName = "proxies.txt",
            DefaultType = ProxyType.Socks4
        });
        var group = await factory.FromOptions(new GroupProxySourceOptions { GroupId = 123 });

        var remoteSource = Assert.IsType<RemoteProxySource>(remote);
        Assert.Equal("https://example.com/proxies.txt", remoteSource.Url);
        Assert.Equal(ProxyType.Socks5, remoteSource.DefaultType);
        var fileSource = Assert.IsType<FileProxySource>(file);
        Assert.Equal("proxies.txt", fileSource.FileName);
        Assert.Equal(ProxyType.Socks4, fileSource.DefaultType);
        var groupSource = Assert.IsType<GroupProxySource>(group);
        Assert.Equal(123, groupSource.GroupId);
    }

    private sealed class StubProxyRepository : IProxyRepository
    {
        public Task AddAsync(ProxyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddAsync(IEnumerable<ProxyEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Attach<TEntity>(TEntity entity) where TEntity : Entity
        {
        }

        public Task DeleteAsync(ProxyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(IEnumerable<ProxyEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public IQueryable<ProxyEntity> GetAll() => Enumerable.Empty<ProxyEntity>().AsQueryable();

        public Task<ProxyEntity> GetAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<ProxyEntity>(null!);

        public Task<int> RemoveDuplicatesAsync(int groupId) => Task.FromResult(0);

        public Task UpdateAsync(ProxyEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(IEnumerable<ProxyEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
