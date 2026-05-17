using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies;
using RuriLib.Models.Jobs.StartConditions;

namespace OpenBullet2.Core.Tests.Models.Jobs;

public class JobOptionsFactoryTests
{
    [Fact]
    public void CreateNew_MultiRun_ReturnsExpectedDefaults()
    {
        var options = Assert.IsType<MultiRunJobOptions>(JobOptionsFactory.CreateNew(JobType.MultiRun));

        Assert.IsType<RelativeTimeStartCondition>(options.StartCondition);
        Assert.Collection(options.HitOutputs, output => Assert.IsType<DatabaseHitOutputOptions>(output));
        var source = Assert.Single(options.ProxySources);
        var groupSource = Assert.IsType<GroupProxySourceOptions>(source);
        Assert.Equal(-1, groupSource.GroupId);
        Assert.False(options.NeverMarkProxiesAsBad);
    }

    [Fact]
    public void CreateNew_ProxyCheck_ReturnsExpectedDefaults()
    {
        var options = Assert.IsType<ProxyCheckJobOptions>(JobOptionsFactory.CreateNew(JobType.ProxyCheck));

        Assert.IsType<RelativeTimeStartCondition>(options.StartCondition);
        Assert.IsType<DatabaseProxyCheckOutputOptions>(options.CheckOutput);
    }

    [Fact]
    public void CloneExistant_MultiRun_DeepClonesAndResetsSkip()
    {
        var options = new MultiRunJobOptions
        {
            Name = "original",
            Skip = 12,
            HitOutputs = [new FileSystemHitOutputOptions { BaseDir = "hits" }]
        };

        var clone = Assert.IsType<MultiRunJobOptions>(JobOptionsFactory.CloneExistant(options));

        Assert.NotSame(options, clone);
        Assert.Equal("original", clone.Name);
        Assert.Equal(0, clone.Skip);
        Assert.NotSame(options.HitOutputs, clone.HitOutputs);
        Assert.IsType<FileSystemHitOutputOptions>(Assert.Single(clone.HitOutputs));
    }

    [Fact]
    public void CloneExistant_ProxyCheck_DeepClones()
    {
        var options = new ProxyCheckJobOptions { Name = "proxy check", Bots = 10 };

        var clone = Assert.IsType<ProxyCheckJobOptions>(JobOptionsFactory.CloneExistant(options));

        Assert.NotSame(options, clone);
        Assert.Equal("proxy check", clone.Name);
        Assert.Equal(10, clone.Bots);
    }
}
