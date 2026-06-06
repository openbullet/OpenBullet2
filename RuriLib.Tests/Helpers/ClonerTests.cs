#nullable enable

using RuriLib.Helpers;
using Xunit;

namespace RuriLib.Tests.Helpers;

public class ClonerTests
{
    [Fact]
    public void Clone_Object_ReturnsDeepClone()
    {
        var original = new TestObject
        {
            Name = "root",
            Child = new TestObject
            {
                Name = "child"
            }
        };

        var cloned = Cloner.Clone(original);

        Assert.NotSame(original, cloned);
        Assert.Equal(original.Name, cloned.Name);
        Assert.NotNull(cloned.Child);
        Assert.NotSame(original.Child, cloned.Child);
        Assert.Equal(original.Child.Name, cloned.Child.Name);
    }

    private sealed class TestObject
    {
        public string Name { get; set; } = string.Empty;
        public TestObject? Child { get; set; }
    }
}
