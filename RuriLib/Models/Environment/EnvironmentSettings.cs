using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Environment;

/// <summary>
/// Settings for customizable data types used in the library.
/// </summary>
public class EnvironmentSettings
{
    /// <summary>
    /// The available wordlist type definitions.
    /// </summary>
    public List<WordlistType> WordlistTypes { get; set; } = [];

    /// <summary>
    /// The custom statuses available to configs and jobs.
    /// </summary>
    public List<CustomStatus> CustomStatuses { get; set; } = [];

    /// <summary>
    /// The export format templates.
    /// </summary>
    public List<ExportFormat> ExportFormats { get; set; } = [];

    /// <summary>
    /// Automatically recognizes a Wordlist Type between the ones available by matching the Regex patterns and returning the first successful match.
    /// </summary>
    /// <param name="data">The data for which you want to recognize the Wordlist Type</param>
    /// <returns>The correct Wordlist Type or (if every Regex match failed) the first one</returns>
    public string RecognizeWordlistType(string data)
    {
        foreach (var type in WordlistTypes)
        {
            if (Regex.Match(data, type.Regex).Success)
            {
                return type.Name;
            }
        }

        return WordlistTypes.First().Name;
    }

    /// <summary>
    /// Parses the EnvironmentSettings from a file.
    /// </summary>
    /// <param name="envFile">The .ini file of the settings</param>
    /// <returns>The loaded EnvironmentSettings object</returns>
    public static EnvironmentSettings FromIni(string envFile)
    {
        var env = new EnvironmentSettings();
        var lines = File.ReadAllLines(envFile).Where(l => !string.IsNullOrEmpty(l)).ToArray();
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.StartsWith("#"))
            {
                continue;
            }

            if (!line.StartsWith("["))
            {
                continue;
            }

            var type = line.Trim() switch
            {
                "[WORDLIST TYPE]" => typeof(WordlistType),
                "[CUSTOM STATUS]" => typeof(CustomStatus),
                "[EXPORT FORMAT]" => typeof(ExportFormat),
                _ => throw new Exception("Unrecognized ini header")
            };

            var header = line;
            var parameters = new List<string>();
            var j = i + 1;
            for (; j < lines.Length; j++)
            {
                line = lines[j];
                if (line.StartsWith("["))
                {
                    break;
                }

                if (line.Trim() == string.Empty || line.StartsWith("#"))
                {
                    continue;
                }

                parameters.Add(line);
            }

            switch (header)
            {
                case "[WORDLIST TYPE]":
                    env.WordlistTypes.Add((WordlistType)ParseObjectFromIni(type, parameters));
                    break;
                case "[CUSTOM STATUS]":
                    env.CustomStatuses.Add((CustomStatus)ParseObjectFromIni(type, parameters));
                    break;
                case "[EXPORT FORMAT]":
                    env.ExportFormats.Add((ExportFormat)ParseObjectFromIni(type, parameters));
                    break;
            }

            i = j - 1;
        }

        return env;
    }

    private static object ParseObjectFromIni(Type type, List<string> parameters)
    {
        var obj = Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Could not create an instance of {type.Name}");

        foreach (var pair in parameters
                     .Where(p => !string.IsNullOrEmpty(p))
                     .Select(p => p.Split('=', 2, StringSplitOptions.TrimEntries)))
        {
            var prop = type.GetProperty(pair[0]);

            if (prop == null)
            {
                throw new Exception($"Property {pair[0]} not found in {type.Name}");
            }

            var propObj = prop.GetValue(obj);
            object? value = propObj switch
            {
                string => pair[1],
                int => int.Parse(pair[1]),
                bool => bool.Parse(pair[1]),
                string[] => pair[1].Split(','),
                _ => null
            };

            prop.SetValue(obj, value);
        }

        return obj;
    }
}
