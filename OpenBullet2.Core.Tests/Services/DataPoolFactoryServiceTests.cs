using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using RuriLib.Models.Data.DataPools;
using RuriLib.Services;

namespace OpenBullet2.Core.Tests.Services;

public sealed class DataPoolFactoryServiceTests : IDisposable
{
    private readonly string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private readonly List<ServiceProvider> serviceProviders = [];

    [Fact]
    public async Task FromOptionsAsync_RangeOptions_ReturnsRangeDataPool()
    {
        var service = CreateService();

        var pool = await service.FromOptionsAsync(new RangeDataPoolOptions
        {
            Start = 5,
            Amount = 3,
            Step = 2,
            Pad = true,
            WordlistType = "Numeric"
        });

        var rangePool = Assert.IsType<RangeDataPool>(pool);
        Assert.Equal(5, rangePool.Start);
        Assert.Equal(3, rangePool.Amount);
        Assert.Equal(2, rangePool.Step);
        Assert.True(rangePool.Pad);
        Assert.Equal("Numeric", rangePool.WordlistType);
        Assert.Equal(["5", "7", "9"], rangePool.DataList.ToArray());
    }

    [Fact]
    public async Task FromOptionsAsync_FileOptions_ReturnsFileDataPool()
    {
        var fileName = Path.Combine(tempDir, "data.txt");
        Directory.CreateDirectory(tempDir);
        await File.WriteAllLinesAsync(fileName, ["one", "two"], TestContext.Current.CancellationToken);
        var service = CreateService();

        var pool = await service.FromOptionsAsync(new FileDataPoolOptions
        {
            FileName = fileName,
            WordlistType = "Default"
        });

        var filePool = Assert.IsType<FileDataPool>(pool);
        Assert.Equal(fileName, filePool.FileName);
        Assert.Equal("Default", filePool.WordlistType);
        Assert.Equal(["one", "two"], filePool.DataList.ToArray());
    }

    [Fact]
    public async Task FromOptionsAsync_FileOptions_WithNullFileName_FallsBackToInfiniteDataPool()
    {
        var options = JsonConvert.DeserializeObject<FileDataPoolOptions>(
            """
            {
              "FileName": null,
              "WordlistType": "Default"
            }
            """)!;

        var service = CreateService();
        var pool = await service.FromOptionsAsync(options);

        Assert.IsType<InfiniteDataPool>(pool);
        Assert.Equal(string.Empty, options.FileName);
    }

    [Fact]
    public async Task FromOptionsAsync_WordlistOptions_ReturnsWordlistDataPool()
    {
        var fileName = Path.Combine(tempDir, "wordlist.txt");
        Directory.CreateDirectory(tempDir);
        await File.WriteAllLinesAsync(fileName, ["alpha", "beta"], TestContext.Current.CancellationToken);
        var entity = new WordlistEntity
        {
            Id = 7,
            Name = "My Wordlist",
            FileName = fileName,
            Type = "Default",
            Total = 2
        };
        var service = CreateService(new FakeWordlistRepository(entity));

        var pool = await service.FromOptionsAsync(new WordlistDataPoolOptions { WordlistId = 7 });

        var wordlistPool = Assert.IsType<WordlistDataPool>(pool);
        Assert.Equal(7, wordlistPool.Wordlist.Id);
        Assert.Equal("My Wordlist", wordlistPool.Wordlist.Name);
        Assert.Equal(["alpha", "beta"], wordlistPool.DataList.ToArray());
    }

    [Fact]
    public async Task FromOptionsAsync_MissingWordlist_FallsBackToInfiniteDataPool()
    {
        var service = CreateService(new FakeWordlistRepository());

        var pool = await service.FromOptionsAsync(new WordlistDataPoolOptions { WordlistId = 404 });

        Assert.IsType<InfiniteDataPool>(pool);
    }

    public void Dispose()
    {
        foreach (var serviceProvider in serviceProviders)
        {
            serviceProvider.Dispose();
        }

        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    private DataPoolFactoryService CreateService(IWordlistRepository? repository = null)
    {
        var repo = repository ?? new FakeWordlistRepository();
        var services = new ServiceCollection();
        services.AddScoped<IWordlistRepository>(_ => repo);

        var serviceProvider = services.BuildServiceProvider();
        serviceProviders.Add(serviceProvider);

        return new DataPoolFactoryService(serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            new RuriLibSettingsService(tempDir));
    }

    private sealed class FakeWordlistRepository(params WordlistEntity[] entities) : IWordlistRepository
    {
        private readonly List<WordlistEntity> entities = [.. entities];

        public Task AddAsync(WordlistEntity entity, CancellationToken cancellationToken = default)
        {
            entities.Add(entity);
            return Task.CompletedTask;
        }

        public Task AddAsync(WordlistEntity entity, MemoryStream stream, CancellationToken cancellationToken = default)
            => AddAsync(entity, cancellationToken);

        public Task DeleteAsync(WordlistEntity entity, bool deleteFile = false, CancellationToken cancellationToken = default)
        {
            entities.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<WordlistEntity> GetAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(entities.FirstOrDefault(e => e.Id == id)!);

        public IQueryable<WordlistEntity> GetAll() => entities.AsQueryable();

        public Task UpdateAsync(WordlistEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Purge() => entities.Clear();

        public void Dispose()
        {
        }
    }
}
