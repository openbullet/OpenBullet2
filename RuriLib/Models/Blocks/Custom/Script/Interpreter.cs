namespace RuriLib.Models.Blocks.Custom.Script;

/// <summary>
/// Interpreters supported by the custom script block.
/// </summary>
public enum Interpreter
{
    /// <summary>
    /// Executes the script through Jint.
    /// </summary>
    Jint,
    /// <summary>
    /// Executes the script through Node.js.
    /// </summary>
    NodeJS,
    /// <summary>
    /// Executes the script through IronPython.
    /// </summary>
    IronPython,
    /// <summary>
    /// Executes the script through CPython via CSnakes.
    /// </summary>
    Python
}
