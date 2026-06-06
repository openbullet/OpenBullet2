using RuriLib.Models.Data;
using Xunit;

namespace RuriLib.Tests.Models.Data;

public class DataPoolDefaultsTests
{
    [Fact]
    public void Defaults_AreSafe()
    {
        var pool = new TestDataPool();

        Assert.NotNull(pool.DataList);
        Assert.Empty(pool.DataList);
        Assert.Equal(string.Empty, pool.WordlistType);
        Assert.Equal(0, pool.Size);
    }

    private sealed class TestDataPool : DataPool;
}
