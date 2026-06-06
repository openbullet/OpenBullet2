using RuriLib.Legacy.Functions.Conditions;
using RuriLib.Legacy.Models;
using RuriLib.Models.Variables;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Legacy.LS;

/// <summary>
/// Represents the legacy variable collection used by the LoliScript runtime.
/// </summary>
public class VariablesList
{
    /// <summary>
    /// Gets the full variable collection.
    /// </summary>
    public List<Variable> Variables { get; private set; }

    /// <summary>
    /// Gets the string variables.
    /// </summary>
    public IEnumerable<StringVariable> Strings
        => Variables.Where(v => v is StringVariable).Cast<StringVariable>();

    /// <summary>
    /// Gets the list variables.
    /// </summary>
    public IEnumerable<ListOfStringsVariable> Lists
        => Variables.Where(v => v is ListOfStringsVariable).Cast<ListOfStringsVariable>();

    /// <summary>
    /// Gets the dictionary variables.
    /// </summary>
    public IEnumerable<DictionaryOfStringsVariable> Dictionaries
        => Variables.Where(v => v is DictionaryOfStringsVariable).Cast<DictionaryOfStringsVariable>();

    /// <summary>
    /// Creates a legacy variable list.
    /// </summary>
    /// <param name="initialVariables">The initial variables to populate the list with.</param>
    public VariablesList(List<Variable>? initialVariables = null)
    {
        Variables = initialVariables ?? new();
    }

    /// <summary>
    /// Gets a variable given its name.
    /// </summary>
    /// <param name="name">The name of the variable</param>
    /// <returns>The variable or null if it wasn't found.</returns>
    public Variable? Get(string name) => Variables.FirstOrDefault(v => v.Name == name);

    /// <summary>
    /// Gets a variable given its name and type.
    /// </summary>
    /// <typeparam name="T">The expected variable type.</typeparam>
    /// <param name="name">The name of the variable</param>
    /// <returns>The variable or null if it wasn't found.</returns>
    public T? Get<T>(string name) where T : Variable => Variables.FirstOrDefault(v => v.Name == name) as T;

    /// <summary>
    /// Helper method that checks if a variable exists given its name.
    /// </summary>
    /// <param name="name">The name of the variable</param>
    /// <returns>True if the variable exists</returns>
    public bool VariableExists(string name) => Variables.Any(v => v.Name == name);

    /// <summary>
    /// Helper method that checks if a variable exists given its name and type.
    /// </summary>
    /// <typeparam name="T">The expected variable type.</typeparam>
    /// <param name="name">The name of the variable</param>
    /// <returns>True if the variable exists and matches the given type</returns>
    public bool VariableExists<T>(string name) where T : Variable
        => Variables.Any(v => v.Name == name && v.GetType() == typeof(T));

    /// <summary>
    /// Replaces any existing variable with the same name and stores the provided variable.
    /// </summary>
    /// <param name="variable">The variable to store.</param>
    public void Set(Variable variable)
    {
        // First of all remove any old variable with the same name
        Remove(variable.Name);

        // Then add the new one
        Variables.Add(variable);
    }

    /// <summary>
    /// Adds a <paramref name="variable"/> to the variables list only if no other variable with the same name exists.
    /// </summary>
    public void SetIfNew(Variable variable)
    {
        if (!VariableExists(variable.Name))
        {
            Set(variable);
        }
    }

    /// <summary>
    /// Removes a variable given its name.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public void Remove(string name) => Variables.RemoveAll(v => v.Name == name);

    /// <summary>
    /// Removes all variables that meet a given a condition.
    /// </summary>
    /// <param name="comparer">The comparison operator.</param>
    /// <param name="name">The comparison term.</param>
    /// <param name="ls">The legacy globals used to resolve variables.</param>
    public void RemoveAll(Comparer comparer, string name, LSGlobals ls)
        => Variables.RemoveAll(v => Condition.ReplaceAndVerify(v.Name, comparer, name, ls));
}
