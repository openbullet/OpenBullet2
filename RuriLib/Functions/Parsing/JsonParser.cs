using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RuriLib.Functions.Parsing;

/// <summary>
/// Parses JSON documents and extracts values by token path.
/// </summary>
public static class JsonParser
{
    /// <summary>
    /// Parses a JSON object or array and extract tokens by path.
    /// </summary>
    /// <param name="json">The serialized JSON object or array</param>
    /// <param name="path">The path to the JToken(s) you want to extract</param>
    /// <returns>The extracted values converted to strings.</returns>
    public static IEnumerable<string> GetValuesByKey(string json, string path)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(path);

        var normalizedJson = json.TrimStart('\uFEFF', ' ', '\t', '\r', '\n');

        if (string.IsNullOrEmpty(normalizedJson))
        {
            throw new ArgumentException("The provided json is not a valid object or array", nameof(json));
        }

        JContainer container = normalizedJson[0] switch
        {
            '{' => JObject.Parse(normalizedJson),
            '[' => JArray.Parse(normalizedJson),
            _ => throw new ArgumentException("The provided json is not a valid object or array", nameof(json)),
        };

        return container.SelectTokens(path, errorWhenNoMatch: false).Select(ConvertToken);
    }

    private static string ConvertToken(JToken token) => token.Type == JTokenType.Float
        ? token.Value<double>().ToString(CultureInfo.InvariantCulture)
        : token.ToString();
}
