namespace OpenBullet2.Web.Dtos.ConfigDebugger;

/// <summary>
/// The list of variables changed.
/// </summary>
public class DbgVariablesChangedMessage
{
    /// <summary>
    /// The list of variables.
    /// </summary>
    public IEnumerable<VariableDto> Variables { get; set; } = Array.Empty<VariableDto>();
}
