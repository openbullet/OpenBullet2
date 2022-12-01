using Newtonsoft.Json;
using System.IO;
using Xunit;

namespace RuriLib.Tests
{
    public class ExportDescriptors
    {
        [Fact]
        public void ExportAllDescriptors()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var json = JsonConvert.SerializeObject(
                Globals.DescriptorsRepository.Descriptors, settings);

            File.WriteAllText("descriptors.json", json);
        }
    }
}
