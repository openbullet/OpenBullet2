using RuriLib.Helpers.CSharp;

namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// DTO that contains a config's script settings.
/// </summary>
public class ConfigScriptSettingsDto
{
    /// <summary>
    /// Defines the additional using statements that the <see cref="ScriptBuilder" /> will use
    /// when building the final script for execution.
    /// </summary>
    public List<string> CustomUsings { get; set; } = new();
}
