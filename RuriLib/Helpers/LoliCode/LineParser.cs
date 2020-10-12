using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RuriLib.Helpers.LoliCode
{
    public static class LineParser
    {
        public static string ParseToken(ref string input)
        {
            input = input.TrimStart();

            var match = Regex.Match(input, "[^ ]*");

            if (!match.Success)
                throw new Exception("Could not parse the token");

            input = input.Substring(match.Value.Length);
            input = input.TrimStart();

            return match.Value;
        }

        public static int ParseInt(ref string input)
        {
            input = input.TrimStart();

            var match = Regex.Match(input, "[0-9]*");

            if (!match.Success)
                throw new Exception("Could not parse the int");

            input = input.Substring(match.Value.Length);
            input = input.TrimStart();

            return int.Parse(match.Value);
        }

        public static float ParseFloat(ref string input)
        {
            input = input.TrimStart();

            var match = Regex.Match(input, "[0-9\\.]*");

            if (!match.Success)
                throw new Exception("Could not parse the int");

            input = input.Substring(match.Value.Length);
            input = input.TrimStart();

            return float.Parse(match.Value, CultureInfo.InvariantCulture);
        }

        public static byte[] ParseByteArray(ref string input)
        {
            input = input.TrimStart();

            var match = Regex.Match(input, "[A-Za-z0-9+/=]+");

            if (!match.Success)
                throw new Exception("Could not parse the byte array");

            input = input.Substring(match.Value.Length);
            input = input.TrimStart();

            return Convert.FromBase64String(match.Value);
        }

        public static bool ParseBool(ref string input)
        {
            input = input.TrimStart();

            var match = Regex.Match(input, "^([Tt]rue)|([Ff]alse)");

            if (!match.Success)
                throw new Exception("Could not parse the bool");

            input = input.Substring(match.Value.Length);
            input = input.TrimStart();

            return bool.Parse(match.Value);
        }

        public static List<string> ParseList(ref string input)
        {
            input = input.TrimStart();

            List<string> list = new List<string>();

            // Syntax of a list of strings: ["one", "two"]
            if (input[0] != '[')
                throw new Exception("Could not parse the list");

            input = input.Substring(1);
            input = input.TrimStart();

            // "one", "two"]
            while (input[0] != ']')
            {
                // , "two"]
                list.Add(ParseLiteral(ref input));
                input = input.TrimStart();

                //  "two"]
                if (input[0] == ',')
                    input = input.Substring(1);

                // "two"]
                input = input.TrimStart();
            }

            // Parse the final ]
            input = input.Substring(1);
            input = input.TrimStart();

            return list;
        }

        public static Dictionary<string, string> ParseDictionary(ref string input)
        {
            input = input.TrimStart();

            Dictionary<string, string> dict = new Dictionary<string, string>();

            // Syntax of a dictionary of strings: { ("key1", "value1"), ("key2", "value2") }
            if (input[0] != '{')
                throw new Exception("Could not parse the dictionary");

            input = input.Substring(1);
            input = input.TrimStart();

            // ("key1", "value1"), ("key2", "value2") }
            while (input[0] != '}')
            {
                if (input[0] != '(')
                    throw new Exception("Could not parse the dictionary");

                input = input.Substring(1);
                input = input.TrimStart();

                // "key1", "value1"), ("key2", "value2") }
                var key = ParseLiteral(ref input);

                // , "value1"), ("key2", "value2") }
                if (input[0] != ',')
                    throw new Exception("Could not parse the dictionary");

                input = input.Substring(1);
                input = input.TrimStart();

                // "value1"), ("key2", "value2") }
                var value = ParseLiteral(ref input);

                dict.Add(key, value);

                // Parse the )
                input = input.Substring(1);
                input = input.TrimStart();

                // , ("key2", "value2") }
                if (input[0] == ',')
                    input = input.Substring(1);

                //  ("key2", "value2") }
                input = input.TrimStart();
            }

            // Parse the final }
            input = input.Substring(1);
            input = input.TrimStart();

            return dict;
        }

        /// <summary>
        /// Parses a literal from the original <paramref name="input"/> and moves forward.
        /// </summary>
        public static string ParseLiteral(ref string input)
        {
            input = input.TrimStart();

            var match = Regex.Match(input, "\"(\\\\.|[^\\\"])*\"");

            if (!match.Success)
                throw new Exception("Could not parse the literal");

            input = input.Substring(match.Value.Length);
            input = input.TrimStart();

            // Wrap the literal in json and deserialize it
            var json = $"{{\"literal\":{match.Value}}}";
            var obj = JObject.Parse(json);
            return (string)obj["literal"];
        }
    }
}
