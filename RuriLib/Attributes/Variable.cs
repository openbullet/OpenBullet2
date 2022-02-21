using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate a parameter of a block method to indicate it should be initialized
    /// as a setting of type variable, optionally with the given default variable name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Variable : Attribute
    {
        /// <summary>
        /// The default variable name to assign as input to this parameter, e.g. data.SOURCE
        /// </summary>
        public string defaultVariableName = null;

        public Variable()
        {

        }

        public Variable(string defaultVariableName)
        {
            this.defaultVariableName = defaultVariableName;
        }
    }
}
