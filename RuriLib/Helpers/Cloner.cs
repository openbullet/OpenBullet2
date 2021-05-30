using Newtonsoft.Json;

namespace RuriLib.Helpers
{
    /// <summary>
    /// Takes care of deep cloning objects.
    /// </summary>
    public static class Cloner
    {
        private static readonly JsonSerializerSettings settings = new() 
        {
            TypeNameHandling = TypeNameHandling.All 
        };

        /// <summary>
        /// Deep clones an object by serializing and deserializing it.
        /// </summary>
        public static T Clone<T>(T obj)
            => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj, settings), settings);
    }
}
