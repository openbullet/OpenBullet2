using RuriLib.Models.Variables;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.LS
{
    public class VariablesList
    {
        public List<Variable> Variables { get; private set; }

        public IEnumerable<StringVariable> Strings
            => Variables.Where(v => v is StringVariable).Cast<StringVariable>();

        public IEnumerable<ListOfStringsVariable> Lists
            => Variables.Where(v => v is ListOfStringsVariable).Cast<ListOfStringsVariable>();

        public IEnumerable<DictionaryOfStringsVariable> Dictionaries
            => Variables.Where(v => v is DictionaryOfStringsVariable).Cast<DictionaryOfStringsVariable>();
        
        public VariablesList(List<Variable> list = null)
        {
            Variables = list ?? new();
        }

        /// <summary>
        /// Gets a variable given its name.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>The variable or null if it wasn't found.</returns>
        public Variable Get(string name) => Variables.FirstOrDefault(v => v.Name == name);

        /// <summary>
        /// Gets a variable given its name and type.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <param name="type">The type of the variable</param>
        /// <returns>The variable or null if it wasn't found.</returns>
        public T Get<T>(string name) where T : Variable => Variables.FirstOrDefault(v => v.Name == name) as T;

        /// <summary>
        /// Helper method that checks if a variable exists given its name.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>True if the variable exists</returns>
        public bool VariableExists(string name) => Variables.Any(v => v.Name == name);

        /// <summary>
        /// Helper method that checks if a variable exists given its name and type.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <param name="type">The type of the variable</param>
        /// <returns>True if the variable exists and matches the given type</returns>
        public bool VariableExists<T>(string name) where T : Variable
            => Variables.Any(v => v.Name == name && v.GetType() == typeof(T));

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
        /// <param name="name">The name of the variable to remove</param>
        public void Remove(string name) => Variables.RemoveAll(v => v.Name == name);
    }
}
