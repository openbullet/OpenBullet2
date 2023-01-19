using RuriLib.Models.Configs.Settings;

namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// DTO that contains a config's input settings.
/// </summary>
public class ConfigInputSettingsDto
{
    /// <summary>
    /// The list of custom inputs, that can be accessed like
    /// <code>input.MYINPUT</code> from the config's script.
    /// </summary>
    public List<CustomInput> CustomInputs { get; set; } = new();
}
