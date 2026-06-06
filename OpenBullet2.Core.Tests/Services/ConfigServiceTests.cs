using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using RuriLib.Models.Configs;
using System.Collections.Concurrent;

namespace OpenBullet2.Core.Tests.Services;

public sealed class ConfigServiceTests : IDisposable
{
    private readonly string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    [Fact]
    public async Task ReloadConfigsAsync_OverlappingReloads_PublishesOnlyTheLatestLocalSnapshot()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var firstResponse = new TaskCompletionSource<IEnumerable<Config>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var repository = new QueuedConfigRepository(
            firstResponse.Task,
            Task.FromResult<IEnumerable<Config>>([CreateConfig("latest")]));
        var service = CreateService(repository);

        var firstReload = service.ReloadConfigsAsync(cancellationToken);
        await repository.WaitForCallCountAsync(1, cancellationToken);

        await service.ReloadConfigsAsync(cancellationToken);

        firstResponse.SetResult([CreateConfig("stale")]);
        await firstReload.WaitAsync(cancellationToken);

        var config = Assert.Single(service.Configs);
        Assert.Equal("latest", config.Id);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    private ConfigService CreateService(IConfigRepository repository)
        => new(repository, new OpenBulletSettingsService(tempDir));

    private static Config CreateConfig(string id)
        => new()
        {
            Id = id,
            Metadata = new ConfigMetadata
            {
                Name = id
            }
        };

    private sealed class QueuedConfigRepository(params Task<IEnumerable<Config>>[] responses) : IConfigRepository
    {
        private readonly ConcurrentQueue<Task<IEnumerable<Config>>> responses = new(responses);
        private readonly TaskCompletionSource firstCallObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int getAllCallCount;

        public async Task WaitForCallCountAsync(int expectedCount, CancellationToken cancellationToken)
        {
            if (expectedCount <= 1)
            {
                await firstCallObserved.Task.WaitAsync(cancellationToken);
                return;
            }

            while (Volatile.Read(ref getAllCallCount) < expectedCount)
            {
                await Task.Delay(10, cancellationToken);
            }
        }

        public Task<Config> CreateAsync(string? id = null) => throw new NotSupportedException();

        public void Delete(Config config) => throw new NotSupportedException();

        public Task<Config> GetAsync(string id) => throw new NotSupportedException();

        public Task<IEnumerable<Config>> GetAllAsync()
        {
            var callCount = Interlocked.Increment(ref getAllCallCount);
            if (callCount == 1)
            {
                firstCallObserved.TrySetResult();
            }

            return responses.TryDequeue(out var response)
                ? response
                : Task.FromResult<IEnumerable<Config>>([]);
        }

        public Task<byte[]> GetBytesAsync(string id) => throw new NotSupportedException();

        public Task SaveAsync(Config config) => throw new NotSupportedException();

        public Task UploadAsync(Stream stream, string fileName) => throw new NotSupportedException();
    }
}
