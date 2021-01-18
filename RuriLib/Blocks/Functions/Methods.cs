using RuriLib.Attributes;
using RuriLib.Models.Bots;
using RuriLib.Providers.UserAgents;

namespace RuriLib.Blocks.Functions
{
    [BlockCategory("Functions", "General purpose functions", "#9acd32")]
    public static class Methods
    {
        [Block("Generates a random User Agent using the builtin provider or a custom list")]
        public static string RandomUserAgent(BotData data, UAPlatform platform = UAPlatform.All)
        {
            data.Logger.LogHeader();
            data.Logger.Log("Getting random UA from the builtin provider");
            string ua;

            try
            {
                ua = data.Providers.RandomUA.Generate(platform);
            }
            catch
            {
                ua = "NO_RANDOM_UA_FOUND";
            }

            data.Logger.Log(ua);
            return ua;
        }
    }
}
