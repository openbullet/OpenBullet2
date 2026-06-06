using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RuriLib.Helpers.Blocks;
using Xunit;

namespace RuriLib.Tests;

public class ExportDescriptors
{
    [Fact]
    public void ExportAllDescriptors()
    {
        var repository = new DescriptorsRepository();
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        var json = JsonConvert.SerializeObject(
            repository.Descriptors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), settings);

        File.WriteAllText("descriptors.json", json);
    }
}
