using RuriLib.Models.Blocks.Custom.Script;

namespace OpenBullet2.Web.Dtos.Config.Blocks;

/// <summary>
/// DTO that represents a script block instance.
/// </summary>
public class ScriptBlockInstanceDto : BlockInstanceDto
{
    /// <summary>
    /// The script to execute.
    /// </summary>
    public string Script { get; set; } = string.Empty;

    /// <summary>
    /// The comma separated list of variables that should be
    /// given to the script as input.
    /// </summary>
    public string InputVariables { get; set; } = string.Empty;

    /// <summary>
    /// The script interpreter.
    /// </summary>
    public Interpreter Interpreter { get; set; }

    /// <summary>
    /// The Python major.minor version used by the CPython interpreter.
    /// </summary>
    public string PythonVersion { get; set; } = "3.12";

    /// <summary>
    /// The variables that should be extracted from the script
    /// and added to the context of the bot.
    /// </summary>
    public List<OutputVariable> OutputVariables { get; set; } = new();
}
