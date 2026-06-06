using OpenBullet2.Core.Services;
using OpenBullet2.Web.Services;
using RuriLib.Models.Jobs;
using RuriLib.Services;
using System.Collections;
using System.Reflection;
using Xunit;

namespace OpenBullet2.Web.Tests.Integration;

public class JobServiceConnectionCleanupTests(ITestOutputHelper testOutputHelper) : IntegrationTests(testOutputHelper)
{
    [Fact]
    public void MultiRunJobService_UnregisterConnection_RemovesTrackedJob_WhenJobWasDeleted()
    {
        var job = CreateMultiRunJob(91001);
        var jobManager = GetRequiredService<JobManagerService>();
        var service = GetRequiredService<MultiRunJobService>();

        jobManager.AddJob(job);
        service.RegisterConnection("conn-1", job.Id);

        Assert.Equal(1, GetTrackedJobCount(service));

        jobManager.RemoveJob(job);
        service.UnregisterConnection("conn-1", job.Id);

        Assert.Equal(0, GetTrackedJobCount(service));
    }

    [Fact]
    public void ProxyCheckJobService_UnregisterConnection_RemovesTrackedJob_WhenJobWasDeleted()
    {
        var job = CreateProxyCheckJob(91002);
        var jobManager = GetRequiredService<JobManagerService>();
        var service = GetRequiredService<ProxyCheckJobService>();

        jobManager.AddJob(job);
        service.RegisterConnection("conn-2", job.Id);

        Assert.Equal(1, GetTrackedJobCount(service));

        jobManager.RemoveJob(job);
        service.UnregisterConnection("conn-2", job.Id);

        Assert.Equal(0, GetTrackedJobCount(service));
    }

    private MultiRunJob CreateMultiRunJob(int id)
        => new(GetRequiredService<RuriLibSettingsService>(), GetRequiredService<PluginRepository>())
        {
            Id = id,
            OwnerId = 0,
            Name = "multi-run-test"
        };

    private ProxyCheckJob CreateProxyCheckJob(int id)
        => new(GetRequiredService<RuriLibSettingsService>(), GetRequiredService<PluginRepository>())
        {
            Id = id,
            OwnerId = 0,
            Name = "proxy-check-test"
        };

    private static int GetTrackedJobCount(object service)
    {
        var field = service.GetType().GetField("_connections", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The service does not expose a _connections field");

        return ((IDictionary)field.GetValue(service)!).Count;
    }
}
