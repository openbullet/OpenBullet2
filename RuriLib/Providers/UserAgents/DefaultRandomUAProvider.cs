using RuriLib.Models.Settings;
using RuriLib.Services;
using System;

namespace RuriLib.Providers.UserAgents;

/// <summary>
/// Default implementation of <see cref="IRandomUAProvider"/>.
/// </summary>
public class DefaultRandomUAProvider : IRandomUAProvider
{
    private readonly GeneralSettings settings;
    private readonly Random rand = new();

    /// <summary>
    /// Creates a provider from the persisted RuriLib settings.
    /// </summary>
    /// <param name="settings">The settings service to read from.</param>
    public DefaultRandomUAProvider(RuriLibSettingsService settings)
    {
        this.settings = settings.RuriLibSettings.GeneralSettings;
    }

    /// <summary>
    /// Gets the total number of configured User-Agents.
    /// </summary>
    public int Total => settings.UserAgents.Count;

    /// <summary>
    /// Generates a random User-Agent.
    /// </summary>
    /// <returns>The generated User-Agent string.</returns>
    public string Generate()
        => settings.UserAgents[rand.Next(Total)];

    /// <summary>
    /// Generates a random User-Agent for the given platform.
    /// </summary>
    /// <param name="platform">The platform family to target.</param>
    /// <returns>The generated User-Agent string.</returns>
    public string Generate(UAPlatform platform)
        => Generate();
}
