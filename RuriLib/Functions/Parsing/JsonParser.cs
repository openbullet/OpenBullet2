using Newtonsoft.Json.Linq;
using RuriLib.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RuriLib.Functions.Parsing
{
    public static class JsonParser
    {
        /// <summary>
        /// Parses a JSON object or array and extract tokens by path.
        /// </summary>
        /// <param name="json">The serialized JSON object or array</param>
        /// <param name="path">The path to the JToken(s) you want to extract</param>
        public static IEnumerable<string> GetValuesByKey(string json, string path)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            JContainer container;

            if (json.StartsWith('{'))
                container = JObject.Parse(json);

            else if (json.StartsWith('['))
                container = JArray.Parse(json);

            else
                throw new ArgumentException("The provided json is not a valid object or array");
            
            return container.SelectTokens(path, false)
                    .Select(ConvertToken);
        }

        private static string ConvertToken(JToken token)
        {
            if (token.Type == JTokenType.Float)
            {
                return token.ToObject<double>().ToString(CultureInfo.InvariantCulture);
            }

            return token.ToString();
        }
    }
}
