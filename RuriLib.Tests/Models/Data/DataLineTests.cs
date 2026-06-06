using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Variables;
using System.Collections.Generic;
using Xunit;

namespace RuriLib.Tests.Models.Data;

public class DataLineTests
{
    private readonly WordlistType wordlistType = new()
    {
        Name = "Default",
        Slices = ["ONE", "TWO"],
        Separator = ","
    };

    [Fact]
    public void GetVariables_ViaWordlistType_Parse()
    {
        DataLine dataLine = new("good,day", wordlistType);
        var variables = dataLine.GetVariables();
        Assert.Equal(2, variables.Count);
        Assert.Equal("ONE", variables[0].Name);
        Assert.Equal("TWO", variables[1].Name);
        Assert.Equal("good", variables[0].AsString());
        Assert.Equal("day", variables[1].AsString());
    }
}
