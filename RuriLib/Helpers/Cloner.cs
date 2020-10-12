using Newtonsoft.Json;

namespace RuriLib.Helpers
{
    public static class Cloner
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        public static T Clone<T>(T obj)
            => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, settings), settings);
    }
}
